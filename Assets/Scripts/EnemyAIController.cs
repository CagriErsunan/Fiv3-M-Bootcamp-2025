using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic; // Needed for Lists
using System.Linq;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyAIController : NetworkBehaviour
{
    // --- A simple state machine for our AI's brain ---
    private enum AIState { Spawning, Patrolling, Chasing, Attacking, Taunting }

    // Synced variable to hold the current state.
    private NetworkVariable<AIState> network_currentState = new NetworkVariable<AIState>(AIState.Patrolling);

    [Header("Patrol Settings")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;
    [SerializeField] private float patrolSpeed = 3f;  
    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private Transform eyeLocation;
    [SerializeField] private LayerMask obstacleLayerMask; // So the enemy can't see through walls
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.0f; // How close the enemy needs to be to attack
    [SerializeField] private float attackCooldown = 1.5f; // Time between attacks
    [SerializeField] private Vector3 attackBoxSize = new Vector3(1, 1, 1); // The size of the "damage" box
    [SerializeField] private Transform attackBoxOrigin; // Where the damage box is created
    private float retargetTimer = 0f;
    private float attackTimer = 0f;
    private bool isAttacking = false;
    [SerializeField] private float retargetInterval = 2.0f;
    private Rigidbody rb;
    private Animator animator;
   // private bool isExecutingAttack = false;
   // private bool hasTaunted = false;
    private Transform patrolTarget;
    private Transform chaseTarget; // The player we are currently chasing
    private Vector3 initialPosition;
    // List of players currently inside our detection trigger
    private List<Transform> playersInDetectionZone = new List<Transform>();

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        if (IsServer)
        {
            // Instead of just playing the animation, we start a coroutine to manage the initial spawn.
            StartCoroutine(InitialSpawnRoutine());
        }

        if (!IsServer) return;
        patrolTarget = patrolPointB;
    }
    private IEnumerator InitialSpawnRoutine()
    {
        // Set the initial state to Spawning.
        network_currentState.Value = AIState.Spawning;

        // Tell clients to play the animation.
        PlaySpawnAnimationClientRpc();

        // Wait for the animation to finish.
        float spawnAnimationLength = 6.0f;
        yield return new WaitForSeconds(spawnAnimationLength);

        // Set the state to Patrolling to begin the AI loop.
        network_currentState.Value = AIState.Patrolling;
    }
    [System.Obsolete]
    public void Respawn()
    {
        // This logic MUST run on the server.
        if (!IsServer) return;

        // Teleport the enemy back to its original starting position.
        // We do this inside a coroutine to handle the physics correctly.
        StartCoroutine(TeleportAndRespawn());
    }

    [System.Obsolete]
    private IEnumerator TeleportAndRespawn()
    {
        // Temporarily disable the AI brain
        network_currentState.Value = AIState.Spawning; // Reset state to avoid weirdness
        

        // Disable physics
        rb.isKinematic = true;

        yield return new WaitForFixedUpdate();

        // Move the object
        transform.position = initialPosition;
        rb.velocity = Vector3.zero;

        yield return new WaitForFixedUpdate();

        // Re-enable physics and the script
        rb.isKinematic = false;
        

        // Play the spawn animation again for everyone.
        PlaySpawnAnimationClientRpc();
        float spawnAnimationLength = 3f;
        yield return new WaitForSeconds(spawnAnimationLength);

        // 3. After waiting, switch the state to Patrolling.
        // The AI will now begin to move.
        network_currentState.Value = AIState.Patrolling;
    }
    [ClientRpc]
    private void PlaySpawnAnimationClientRpc()
    {
        animator.SetTrigger("Spawn");
    }
    [System.Obsolete]
    private void FixedUpdate()
    {
        // This logic only runs on the server.
        if (!IsServer) return;

        // If the attack sequence coroutine is running, freeze the AI's brain.
        if (isAttacking)
        {
            return;
        }

        // The attack cooldown timer ticks down continuously.
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // The AI's brain simply calls the appropriate function for its current state.
        switch (network_currentState.Value)
        {
            case AIState.Spawning:
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                animator.SetFloat("Speed", 0);
                break;

            case AIState.Patrolling:
                HandlePatrolling();
                LookForPlayer();
                break;

            case AIState.Chasing:
                HandleChasing();
                break;

            // --- THE FIX IS HERE ---
            // We need to tell the brain what to do when it enters the Attacking state.
            case AIState.Attacking:
                HandleAttacking();
                break;
        }
    }

    // --- New Detection and State Functions ---

    private void LookForPlayer()
    {
        if (playersInDetectionZone.Count > 0)
        {
            foreach (var playerTransform in playersInDetectionZone)
            {
                if (HasLineOfSight(playerTransform))
                {
                    chaseTarget = playerTransform;
                    network_currentState.Value = AIState.Chasing;
                    return;
                }
            }
        }
    }

    // This function is called by the DetectionZone's trigger.
    public void OnPlayerEnterDetectionZone(Transform playerTransform)
    {
        if (!IsServer) return;
        // Add the player to our list of potential targets.
        if (!playersInDetectionZone.Contains(playerTransform))
        {
            playersInDetectionZone.Add(playerTransform);
        }
    }

    public void OnPlayerExitDetectionZone(Transform playerTransform)
    {
        if (!IsServer) return;
        // Remove the player from our list.
        playersInDetectionZone.Remove(playerTransform);
    }

    // --- New Movement Handlers ---

    private void HandlePatrolling()
    {
        // This is your original patrolling code, now in its own function.
        if (patrolPointA == null || patrolPointB == null || patrolTarget == null) return;

        Vector3 directionToTarget = (patrolTarget.position - transform.position);
        directionToTarget.y = 0;
        directionToTarget.Normalize();

        rb.linearVelocity = new Vector3(directionToTarget.x * patrolSpeed, rb.linearVelocity.y, 0);
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
        if (rb.linearVelocity.magnitude > 0.1f) transform.rotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, 0));

        float distanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(patrolTarget.position.x, 0, patrolTarget.position.z));
        if (distanceToTarget < 1.0f)
        {
            patrolTarget = (patrolTarget == patrolPointA) ? patrolPointB : patrolPointA;
        }
    }

    // Inside EnemyAIController.cs
    // Inside EnemyAIController.cs, add this entire new function.

    private void OnDrawGizmos()
    {
        // Don't draw if we're not chasing anyone.
        if (network_currentState.Value != AIState.Chasing || chaseTarget == null) return;

        // Use the exact same logic as our HasLineOfSight function
        Vector3 startPoint = eyeLocation.position;
        Vector3 endPoint = chaseTarget.position;
        endPoint.y = startPoint.y; // Flatten
        Vector3 direction = (endPoint - startPoint).normalized;
        float distance = Vector3.Distance(startPoint, endPoint);

        RaycastHit hit;
        if (Physics.Raycast(startPoint, direction, out hit, distance, obstacleLayerMask))
        {
            // Draw a RED line from eyes to the obstacle we hit.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startPoint, hit.point);
        }
        else
        {
            // Draw a GREEN line all the way to the player's position (on the same plane).
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attackBoxOrigin == null) return;

        Gizmos.color = Color.yellow;
        // Note: Gizmos.DrawCube needs the FULL size, not half-extents
        Gizmos.DrawWireCube(attackBoxOrigin.position, attackBoxSize);
    }
    [System.Obsolete]
    private void HandleChasing()
    {
        // --- Phase 1: Check if our current target is still valid ---
        // This part is mostly the same, but we will use a 2D distance check here too.
        if (chaseTarget == null || !HasLineOfSight(chaseTarget))
        {
            chaseTarget = null;
            network_currentState.Value = AIState.Patrolling;
            Debug.Log("AI lost line of sight. Returning to patrol.");
            return;
        }

        // --- THE FIX: Calculate distance on a 2D plane ---
        Vector3 flatEnemyPos = new Vector3(transform.position.x, 0, 0);
        Vector3 flatTargetPos = new Vector3(chaseTarget.position.x, 0, 0);
        float distanceToTarget2D = Vector3.Distance(flatEnemyPos, flatTargetPos);

        // Now check the distance.
        if (distanceToTarget2D > 15f)
        {
            chaseTarget = null;
            network_currentState.Value = AIState.Patrolling;
            Debug.Log("AI target is too far. Returning to patrol.");
            return;
        }


        // --- Phase 2: Dynamic Re-targeting Logic (No changes needed here) ---
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;
            Transform bestTarget = chaseTarget;
            // NOTE: We'll use the 2D distance for the re-targeting comparison as well, making it smarter.
            float bestTargetDistance = Vector3.Distance(new Vector3(transform.position.x, 0, 0), new Vector3(bestTarget.position.x, 0, 0));

            foreach (var potentialTarget in playersInDetectionZone)
            {
                if (potentialTarget != null && HasLineOfSight(potentialTarget))
                {
                    float potentialTargetDistance = Vector3.Distance(new Vector3(transform.position.x, 0, 0), new Vector3(potentialTarget.position.x, 0, 0));
                    if (potentialTargetDistance < bestTargetDistance)
                    {
                        bestTarget = potentialTarget;
                        bestTargetDistance = potentialTargetDistance;
                    }
                }
            }
            chaseTarget = bestTarget;
        }


        // --- THE FIX: Check for Attack Range using our 2D distance ---
        if (distanceToTarget2D <= attackRange && attackTimer <= 0)
        {
            // Set the attack timer HERE before changing state to prevent multiple attack calls.
            attackTimer = attackCooldown;
            // Switch to the Attacking state. The FixedUpdate brain will handle it next frame.
            network_currentState.Value = AIState.Attacking;
            return;
        }

        // --- Phase 3: Movement and Animation (No changes needed here) ---
        Vector3 directionToTarget = (chaseTarget.position - transform.position).normalized;
        rb.velocity = new Vector3(directionToTarget.x * chaseSpeed, rb.velocity.y, 0); // Changed to .velocity from .linearVelocity
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, 0));
        }
    }

    [System.Obsolete]
    private void HandleAttacking()
    {
        // Phase 1: Stop the enemy and make sure it's facing its target.
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        animator.SetFloat("Speed", 0);

        if (chaseTarget != null)
        {
            Vector3 direction = (chaseTarget.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, 0));
        }

        // Phase 2: Lock the brain and hand off control to the coroutine.
        // The 'isAttacking' flag will freeze the FixedUpdate loop.
        isAttacking = true;

        // Start the coroutine that will handle the full attack-damage-taunt sequence.
        StartCoroutine(AttackAndTauntSequence());
    }

    [System.Obsolete]
    private IEnumerator AttackAndTauntSequence()
    {
        // --- 1. ATTACK ---
        animator.SetTrigger("Attack");
        // Wait for the moment in the animation where the swing should do damage.
        yield return new WaitForSeconds(0.25f);

        // --- 2. DAMAGE CHECK & RESPAWN ---
        bool didHitPlayer = false;
        Vector3 attackBoxCenter = attackBoxOrigin.position;
        Collider[] hits = Physics.OverlapBox(attackBoxCenter, attackBoxSize / 2, transform.rotation);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerController>(out PlayerController player))
            {
                // Use the reliable ServerRpc to tell the client to respawn.
                player.Respawn();
                didHitPlayer = true;
                break; // Stop after hitting one player.
            }
        }

        // Wait for the rest of the attack animation to finish.
        yield return new WaitForSeconds(0.5f);

        // --- 3. TAUNT (if we hit someone) ---
        if (didHitPlayer)
        {
            animator.SetTrigger("Taunt");
            // Wait for the taunt animation to finish.
            yield return new WaitForSeconds(2.0f);
        }

        // --- 4. COOLDOWN AND UNLOCK ---
        // Reset the cooldown timer.
        attackTimer = attackCooldown;

        // Tell the AI to go back to chasing.
        network_currentState.Value = AIState.Chasing;

        // Unlock the brain so FixedUpdate can run again.
        isAttacking = false;
    }
   /* [System.Obsolete]
    private IEnumerator AttackSequence()
    {
        // --- Phase 1: The Attack ---
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.25f); // Wait for the damage frame

        // --- Perform the Damage Check ---
        Vector3 attackBoxCenter = attackBoxOrigin.position;
        Collider[] hits = Physics.OverlapBox(attackBoxCenter, attackBoxSize / 2, transform.rotation);
        bool didHitPlayer = false;
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerController>(out PlayerController player))
            {
                // THE FIX: Use the public ServerRpc on the PlayerController.
                // This is the reliable way to tell a client to respawn.
                player.Respawn();
                didHitPlayer = true;
                break; // We only need to hit one player.
            }
        }

        // Wait for the rest of the attack animation to finish.
        yield return new WaitForSeconds(0.5f);

        // --- Phase 2: The Taunt ---
        if (didHitPlayer)
        {
            // We don't need to change the state here, just play the animation.
            animator.SetTrigger("Taunt");
            // Wait for the taunt animation to finish.
            yield return new WaitForSeconds(2.0f);
        }

        // --- Phase 3: Cooldown, Reset State, and Unlock Brain ---
        attackTimer = attackCooldown;

        // Tell the AI to go back to chasing its last known target.
        network_currentState.Value = AIState.Chasing;

        // IMPORTANT: Unlock the AI's brain so FixedUpdate can run again.
        isAttacking = false;
    }*/
 /*   [System.Obsolete]
    private IEnumerator PerformAttackDamageCheck(float delay)
    {
       // isExecutingAttack = true;

        // Wait for the specified delay.
        yield return new WaitForSeconds(delay);

        // After waiting, now we perform the damage check.
        // This code is moved from HandleAttacking.
        Vector3 attackBoxCenter = attackBoxOrigin.position;
        Collider[] hits = Physics.OverlapBox(attackBoxCenter, attackBoxSize / 2, transform.rotation);
        bool didHitPlayer = false;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<PlayerController>(out PlayerController player))
            {
                Debug.Log($"AI hit Player {player.OwnerClientId}");
                player.Respawn();
                didHitPlayer = true;
            }
        }

        // Now, decide the next state.
        if (didHitPlayer)
        {
            network_currentState.Value = AIState.Taunting;
        }
        else
        {
            // If we missed, just go back to chasing after a short delay.
            yield return new WaitForSeconds(0.5f); // Wait a bit for the animation to finish
            network_currentState.Value = AIState.Chasing;
        }
        
    }*/

   /* [System.Obsolete]
    private void HandleTaunting()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        animator.SetFloat("Speed", 0);

        // THE FIX: Use the new flag.
        if (!hasTaunted)
        {
            // If we haven't taunted yet in this state, trigger the animation.
            animator.SetTrigger("Taunt");
            // Then, immediately set the flag to true so this doesn't run again.
            hasTaunted = true;
        }

        // The rest of the logic is the same.
        if (attackTimer <= 0)
        {
            network_currentState.Value = AIState.Chasing;
        }
    }*/
    private bool HasLineOfSight(Transform target)
    {
        // If the target is null, we can't see it.
        if (target == null) return false;

        // Get the start position of our raycast (from the enemy's eyes).
        Vector3 startPoint = eyeLocation.position;
        // Get the position of the target.
        Vector3 endPoint = target.position;

        // IMPORTANT: Flatten the Y-coordinate to match. This ensures a horizontal check.
        // We check line of sight at the same height as our eyes.
        endPoint.y = startPoint.y;

        // Calculate direction and distance on this flat plane.
        Vector3 direction = (endPoint - startPoint).normalized;
        float distance = Vector3.Distance(startPoint, endPoint);

        // Perform the raycast.
        RaycastHit hit;
        if (Physics.Raycast(startPoint, direction, out hit, distance, obstacleLayerMask))
        {
            // If the raycast hits something on the obstacle layer, our sight is blocked.
            return false;
        }

        // If the raycast hits nothing, we have a clear line of sight.
        return true;
    }
}