using Unity.Netcode;
using UnityEngine;
using System.Collections; // We will use coroutines for attack patterns
using System.Collections.Generic;
using TMPro;
// This is the brain of our stationary boss.
public class BossAIController : NetworkBehaviour
{
    // A simple state machine to control what the boss is doing.
    private enum BossState
    {
        Idle,       // Waiting to attack
        Attacking   // Currently performing an attack
    }

    [Header("Attack Settings")]
    [SerializeField] private float timeBetweenAttacks = 5f; // Time the boss waits in Idle state
    [Header("Line Attack")]
    [SerializeField] private GameObject slowZonePrefab;
    [SerializeField] private GameObject lineAttackVFXPrefab;
    [SerializeField] private GameObject lineWarningVFXPrefab; // <-- ADD a slot for the warning beam
    [SerializeField] private float lineAttackWarningDuration = 1.5f; // How long the warning stays
    [Header("Component References")]
    [SerializeField] private Transform projectileSpawnPoint; // Where projectiles are created
    [SerializeField] private GameObject bossProjectilePrefab; // The projectile prefab we made
    [Header("Behavior")]
   // [SerializeField] private Transform eyeTransform;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private Transform eyeTransform;
    private Transform nearestPlayer = null;// --- NEW VARIABLES for the Ground Slam attack ---
    [Header("Ground Slam Attack")]
    [SerializeField] private GameObject groundWarningPrefab; // The red circle visual effect
    [SerializeField] private GameObject groundExplosionPrefab; // The networked damage trigger
    [SerializeField] private int numberOfExplosions = 5;
    [SerializeField] private float warningDuration = 2f; // How long the warning shows before the boom
    [SerializeField] private Vector2 arenaSize = new Vector2(20, 20);
    [SerializeField] private GameObject explosionVFXPrefab; // The X and Z size of the playable area
    private Animator animator;
    private int currentDifficultyTier = 0;
    private GameManager_BossArena gameManager;

    // A synced variable to track the boss's current state.
    // Only the server can change it.
    private NetworkVariable<BossState> network_currentState = new NetworkVariable<BossState>(BossState.Idle);

    // A timer to control the flow of attacks.
    private float attackTimer;
    public void SetDifficulty(int tier)
    {
        // This must only run on the server.
        if (!IsServer) return;

        currentDifficultyTier = tier;
    }

    // This function runs once when the boss is spawned on the network.
    [System.Obsolete]
    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();

        // This AI logic should only ever run on the server.
        if (!IsServer) return;
        if (IsServer)
        {
            gameManager = FindObjectOfType<GameManager_BossArena>();
        }
        // When the boss spawns, start its "thinking" process.
        StartCoroutine(BossLogicRoutine());
    }

    // This is the main "brain" loop for the boss.
    private IEnumerator BossLogicRoutine()
    {
        while (true)
        {
            // FASTER attacks based on difficulty!
            float waitTime = timeBetweenAttacks - (currentDifficultyTier * 0.5f);
            if (waitTime < 1.5f) waitTime = 1.5f; // Set a minimum wait time

            yield return new WaitForSeconds(waitTime);

            ChooseAndPerformAttack();
        }
    }

    private void ChooseAndPerformAttack()
    {
        // --- THE NEW RANDOM CHOICE LOGIC ---
        // Create a list of available attacks.
        List<System.Action> attacks = new List<System.Action>();
        attacks.Add(() => StartCoroutine(RadialBurstAttack()));
        attacks.Add(() => StartCoroutine(GroundSlamAttack()));
        attacks.Add(() => StartCoroutine(LineAttack()));
        // Choose one randomly from the list.
        int randomIndex = Random.Range(0, attacks.Count);

        // Execute the chosen attack.
        attacks[randomIndex].Invoke();

    }
    private IEnumerator LineAttack()
    {
        // --- Phase 1: Preparation ---
        network_currentState.Value = BossState.Attacking;
        PlayAnimationTriggerClientRpc("AttackLine");

        // This is the "wind-up" time for the animation before anything happens.
        // Let's make it consistent.
        yield return new WaitForSeconds(0.75f);

        Debug.Log("Server: Firing TELEGRAPHED Line Attack!");

        // --- Phase 2: Aiming ---
        // Find the nearest player to target.
        FindNearestPlayer();
        if (nearestPlayer == null)
        {
            // If there's no one to target, just end the attack early.
            network_currentState.Value = BossState.Idle;
            yield break;
        }

        // Calculate the targeted position and rotation for the attack.
        Vector3 directionToPlayer = nearestPlayer.position - eyeTransform.position;
        directionToPlayer.y = 0;
        directionToPlayer.Normalize();
        Vector3 targetPosition = transform.position + directionToPlayer * 10f;
        targetPosition.y = 0.1f;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);


        // --- Phase 3: Warning (with Difficulty Scaling) ---
        // Calculate how long the warning should last based on the current difficulty.
        float actualWarningDuration = lineAttackWarningDuration - (currentDifficultyTier * 0.2f);
        // Ensure the warning time never becomes too short to react to.
        if (actualWarningDuration < 0.5f)
        {
            actualWarningDuration = 0.5f;
        }

        // Tell all clients to show the warning beam for the calculated duration.
        ShowLineWarningClientRpc(targetPosition, targetRotation, actualWarningDuration);

        // Wait for the warning period to end. This is where the players have time to dodge.
        yield return new WaitForSeconds(actualWarningDuration);


        // --- Phase 4: Damage and Effects ---
        // Now that the warning is over, spawn the actual damage zone.
        GameObject slowZoneGO = Instantiate(slowZonePrefab, targetPosition, targetRotation);
        slowZoneGO.GetComponent<NetworkObject>().Spawn();
        Destroy(slowZoneGO, 0.5f); // The damage zone only needs to exist for a moment.

        // And tell all clients to play the "hit" visual effect.
        SpawnLineAttackVFXClientRpc(targetPosition, targetRotation);


        // --- Phase 5: Cooldown ---
        // A brief final pause to let the animations and effects finish.
        yield return new WaitForSeconds(1.0f);
        // Set the state back to Idle so the main brain can start the timer for the next attack.
        network_currentState.Value = BossState.Idle;
    }
    // The new ShowLineWarningClientRpc

    [ClientRpc]
    private void ShowLineWarningClientRpc(Vector3 position, Quaternion rotation, float duration)
    {
        if (lineWarningVFXPrefab != null)
        {
            // The warning will now destroy itself after the correct, scaled duration.
            Destroy(Instantiate(lineWarningVFXPrefab, position, rotation), duration);
        }
    }
    public void StopAI()
    {
        if (!IsServer) return;

        // Stop all running coroutines (like attack patterns).
        StopAllCoroutines();

        // Set the state to a safe, idle state.
        network_currentState.Value = BossState.Idle;

        // You could also tell the animator to play a "power down" or "death" animation here.
        // For now, just stopping is enough.
    }
    [ClientRpc]
    private void ShowLineWarningClientRpc(Vector3 position, Quaternion rotation)
    {
        if (lineWarningVFXPrefab != null)
        {
            // Each client creates their own local warning beam.
            // It's destroyed after the warning duration.
            Destroy(Instantiate(lineWarningVFXPrefab, position, rotation), lineAttackWarningDuration);
        }
    }
    [ClientRpc]
    private void SpawnLineAttackVFXClientRpc(Vector3 position, Quaternion rotation)
    {
        if (lineAttackVFXPrefab != null)
        {
            // Each client creates their own local visual effect.
            Destroy(Instantiate(lineAttackVFXPrefab, position, rotation), 2.0f);
        }
    }
    private IEnumerator GroundSlamAttack()
    {
        // Phase 1: Preparation (is the same)
        network_currentState.Value = BossState.Attacking;
        PlayAnimationTriggerClientRpc("AttackGroundSlam");
        yield return new WaitForSeconds(0.5f);

        // Phase 2: Create Warnings (THE NEW LOGIC)
        Debug.Log("Server: Starting SMART Ground Slam attack!");
        List<Vector3> explosionPositions = new List<Vector3>();

        if (gameManager != null)
        {
            // Find positions for the explosions using the AI's "brain".
            for (int i = 0; i < numberOfExplosions; i++)
            {
                // Ask the GameManager for the current hottest spot.
                Vector3 position = gameManager.GetHottestTargetPosition();

                // Add a little bit of random offset so the attacks aren't always in the exact center of a tile.
                position.x += Random.Range(-1f, 1f);
                position.z += Random.Range(-1f, 1f);

                explosionPositions.Add(position);
            }
        }
        else
        {
            // Fallback to the old random logic if the manager isn't found.
            for (int i = 0; i < numberOfExplosions; i++)
            {
                float randomX = Random.Range(-arenaSize.x / 2, arenaSize.x / 2);
                float randomZ = Random.Range(-arenaSize.y / 2, arenaSize.y / 2);
                explosionPositions.Add(new Vector3(randomX, 0, randomZ));
            }
        }


        // The rest of the function is exactly the same as before.
        // It takes the list of positions and creates the warnings and explosions.
        ShowGroundWarningsClientRpc(explosionPositions.ToArray());
        yield return new WaitForSeconds(warningDuration);

        foreach (Vector3 pos in explosionPositions)
        {
            GameObject explosionGO = Instantiate(groundExplosionPrefab, pos, Quaternion.identity);
            explosionGO.GetComponent<NetworkObject>().Spawn();
            Destroy(explosionGO, 0.5f);
            SpawnExplosionVFXClientRpc(pos);
        }

        yield return new WaitForSeconds(1.0f);
        network_currentState.Value = BossState.Idle;
    }
    [ClientRpc]
    private void SpawnExplosionVFXClientRpc(Vector3 position)
    {
        // This code runs on EVERY client's machine (and the host).
        if (explosionVFXPrefab != null)
        {
            // Each client is responsible for creating their own local instance of the effect.
            Destroy(Instantiate(explosionVFXPrefab, position, Quaternion.identity), 2.0f);
        }
    }
    [ClientRpc]
    private void ShowGroundWarningsClientRpc(Vector3[] positions)
    {
        // This code runs on EVERY client's machine.
        Debug.Log($"Client received {positions.Length} warning positions.");
        foreach (Vector3 pos in positions)
        {
            // Each client creates their own LOCAL, purely visual warning effect.
            // We'll have it destroy itself after the warning duration.
            Destroy(Instantiate(groundWarningPrefab, pos, Quaternion.identity), warningDuration);
        }
    }
    // The new, simple Update function

    private void Update()
    {
        // This logic only runs on the server.
        if (!IsServer) return;

        // First, find the nearest player.
        FindNearestPlayer();

        // If we have a valid target...
        if (nearestPlayer != null)
        {
            // 1. Calculate the direction from the boss to the player.
            Vector3 directionToPlayer = nearestPlayer.position - transform.position;

            // 2. IMPORTANT: Flatten the direction vector so the boss doesn't tilt up or down.
            directionToPlayer.y = 0;

            // 3. Make sure we have a valid direction to look in.
            if (directionToPlayer != Vector3.zero)
            {
                // 4. Calculate the rotation that "looks" in that direction.
                // Because your eye is on the Z-axis, this will work perfectly.
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // 5. Smoothly rotate towards that target rotation over time.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    private void FindNearestPlayer()
    {
        float minDistance = float.MaxValue;
        Transform closestPlayer = null;

        // Loop through all connected clients.
        foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            if (client.PlayerObject != null)
            {
                // Make sure the player is not eliminated.
                PlayerController pc = client.PlayerObject.GetComponent<PlayerController>();
                if (pc != null && !pc.IsEliminated.Value)
                {
                    float distance = Vector3.Distance(transform.position, client.PlayerObject.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPlayer = client.PlayerObject.transform;
                    }
                }
            }
        }

        nearestPlayer = closestPlayer;
    }
    private IEnumerator RadialBurstAttack()
    {
        // --- Phase 1: Preparation (Same as before) ---
        network_currentState.Value = BossState.Attacking;
        PlayAnimationTriggerClientRpc("AttackRadialBurst");
        yield return new WaitForSeconds(0.5f); // Wind-up time

        // --- Phase 2: Firing the Projectiles (NEW LOGIC) ---
        Debug.Log("Server: Firing ENHANCED Radial Burst!");

        int numberOfProjectiles = 16 + (currentDifficultyTier * 4);
        float angleStep = 360f / numberOfProjectiles;
        float mediumWaveOffset = angleStep / 2;
        // --- NEW: Randomly choose the attack type ---
        int attackType = Random.Range(0, 3); // 0 = Low, 1 = High, 2 = Both

        // --- Fire the Low Burst ---
        if (attackType == 0 || attackType == 2) // Fire if type is Low or Both
        {
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float currentAngle = angleStep * i;
                // Rotation is flat along the Y-axis (horizontal).
                Quaternion projectileRotation = Quaternion.Euler(0, currentAngle, 0);

                // Spawn at the normal height.
                Vector3 spawnPosition = projectileSpawnPoint.position;

                GameObject projectileGO = Instantiate(bossProjectilePrefab, spawnPosition, projectileRotation);
                projectileGO.GetComponent<NetworkObject>().Spawn();
            }
        }

        // --- Fire the High Burst ---
        if (attackType == 1 || attackType == 2) // Fire if type is High or Both
        {
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                float currentAngle = angleStep * i;
                // --- THE CHANGE ---
                // We add a slight upward angle (e.g., -15 on X-axis) to the rotation.
                // A negative X rotation in Unity points the object upwards.
                Quaternion projectileRotation = Quaternion.Euler(-5, currentAngle, 0);

                // Spawn at the same position.
                Vector3 spawnPosition = projectileSpawnPoint.position;

                GameObject projectileGO = Instantiate(bossProjectilePrefab, spawnPosition, projectileRotation);
                projectileGO.GetComponent<NetworkObject>().Spawn();
            }
        }
        if (attackType == 2)
        {
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                // We add our offset to the angle to fire into the gaps.
                float currentAngle = (angleStep * i) + mediumWaveOffset;
                Quaternion projectileRotation = Quaternion.Euler(-1, currentAngle, 0); // A medium angle, e.g., -5
                Vector3 spawnPosition = projectileSpawnPoint.position;
                GameObject projectileGO = Instantiate(bossProjectilePrefab, spawnPosition, projectileRotation);
                projectileGO.GetComponent<NetworkObject>().Spawn();
            }
        }
        // --- Phase 3: Cooldown (Same as before) ---
        yield return new WaitForSeconds(1.0f);
        network_currentState.Value = BossState.Idle;
    }
    [ClientRpc]
    private void PlayAnimationTriggerClientRpc(string triggerName)
    {
        // This runs on all clients. It tells their local animator to fire a trigger.
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}