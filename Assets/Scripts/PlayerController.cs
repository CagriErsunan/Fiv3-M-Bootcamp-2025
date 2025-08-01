using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkAnimator))]
[RequireComponent(typeof(Animator))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeed = 8.0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float pushSpeed = 2.0f;
    [Header("Health & Stats")]
    [SerializeField] public int maxHealth = 2;
    [Header("Predator Skill")]
    [SerializeField] private float hasteDuration = 3.0f;
    [SerializeField] private float hasteCooldown = 10.0f;
    [SerializeField] private float hasteSpeedMultiplier = 1.5f; // 50% faster
                                                                // A reference to the UI element we created.
    private HasteSkillUIController hasteSkillUI;

    // A reference to our detection zone script.
    private PredatorZoneController predatorZone;
    // Local variables to manage the skill's state.
    private float hasteCooldownTimer = 0f;
    private bool isHasteActive = false;
    //private bool uiInitialized = false;
    [Header("Wall Jump")]
    [SerializeField] private float wallDetectionDistance = 0.7f;
    [SerializeField] private float wallJumpForce = 7f;
    [SerializeField] private LayerMask whatIsWall; // A new layer mask to define what we can wall jump off
    [Header("Cosmetic Containers")]
    [SerializeField] private Transform hatsContainer;
    [SerializeField] private Transform pantsContainer;
    [SerializeField] private Transform shoesContainer;
    [SerializeField] private Transform tshirtsContainer;
    [SerializeField] private Transform costumesContainer;
    [SerializeField] private Transform glassesContainer;
    [SerializeField] private Transform facesContainer;
    [SerializeField] private Transform glovesContainer;
    [Header("Pushing")]
    [SerializeField] private float pushRaycastDistance = 0.7f;
    private PushableBox lastPushedBox; // Keep track of the last box we pushed
    // A new variable for the player's points.
    // Only the Server can change it, but everyone can read it.
    private NetworkVariable<bool> network_isCrouching = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> network_isGliding = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Health = new NetworkVariable<int>();
    public NetworkVariable<bool> IsEliminated = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> network_isPushing = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> HatIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> PantsIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> ShoesIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> TshirtIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> CostumeIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> GlassesIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> FacesIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> GlovesIndex = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> PlayerScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(PlayerRole.Survivor, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Rigidbody rb;
    private Renderer characterRenderer;
    // private Color originalColor;
    // private bool originalColorIsSet = false;
    private Animator animator;
    private Transform mainCameraTransform;
    private bool cameraIsSetUp = false;
    private float currentSpeed;
    private float speedMultiplier = 1.0f;
    private bool onLaunchPadCooldown = false;
    private bool isLaunchLocked = false;
    private bool controlsAreLocked = false;
    private NetworkVariable<float> networkAnimatorSpeed = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private bool hasWonThisRound = false;
    ///////
    [Header("Pushing")]
    public NetworkVariable<bool> isEliminatedTepsi = new NetworkVariable<bool>(false);
    private Collider col;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject spectatorCamera;
    ///////
    [SerializeField] private CosmeticDatabase cosmeticDatabase;
    [System.Obsolete]


    public override void OnNetworkSpawn()
    {
        // --- NEW, BETTER COLOR LOGIC ---
        // We specifically find the "Stickman" object which holds the main body renderer.
        Transform bodyTransform = transform.Find("Stickman");
        if (bodyTransform != null)
        {
            characterRenderer = bodyTransform.GetComponent<Renderer>();
        }
        else
        {
            Debug.LogError("Could not find 'Stickman' child on player!", this.gameObject);
        }
        if (IsOwner)
        {
            mainCameraTransform = Camera.main.transform;
        }
        // --- End of NEW COLOR LOGIC ---
        if (IsServer)
        {
            Health.Value = maxHealth;
            IsEliminated.Value = false;
        }
        Health.OnValueChanged += OnHealthChanged;

        // The rest of your code is correct and stays the same.
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        network_isGliding.OnValueChanged += OnIsGlidingChanged;
        HatIndex.OnValueChanged += OnHatChanged;
        PantsIndex.OnValueChanged += OnPantsChanged;
        ShoesIndex.OnValueChanged += OnShoesChanged;
        TshirtIndex.OnValueChanged += OnTshirtChanged;
        CostumeIndex.OnValueChanged += OnCostumeChanged;
        GlassesIndex.OnValueChanged += OnGlassesChanged;
        FacesIndex.OnValueChanged += OnFacesChanged;
        GlovesIndex.OnValueChanged += OnGlovesChanged;
        // Subscribe to the change event for our new variable.
        network_isPushing.OnValueChanged += OnIsPushingChanged;
        network_isCrouching.OnValueChanged += OnIsCrouchingChanged;
        // Call it once to set the initial state.
        OnIsPushingChanged(false, network_isPushing.Value);
        Role.OnValueChanged += OnRoleChanged;
        // We still call this once to set the initial state correctly.
        OnRoleChanged(PlayerRole.Survivor, Role.Value);
        /*if (!IsOwner)
        {
            network_isPushing.OnValueChanged += OnIsPushingChanged;
        }*/
        if (IsOwner)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
        }
        predatorZone = GetComponentInChildren<PredatorZoneController>();



        // Subscribe to role changes to show/hide the UI.
        Role.OnValueChanged += HandleRoleForSkillUI;
        // Call it once to set the initial state.
        // HandleRoleForSkillUI(PlayerRole.Survivor, Role.Value);

        //////////
        if (playerCamera != null)
            playerCamera.SetActive(true);

        if (spectatorCamera != null)
            spectatorCamera.SetActive(false);
    }
    private void HandleRoleForSkillUI(PlayerRole previousRole, PlayerRole newRole)
    {
        Debug.Log($"HandleRoleForSkillUI called. IsOwner: {IsOwner}, New Role: {newRole}");
        if (hasteSkillUI != null && IsOwner)
        {
            hasteSkillUI.gameObject.SetActive(newRole != PlayerRole.Survivor);
        }
        /*   else if (IsOwner)
           {
               // Add this log in case the UI reference is the problem.
               Debug.LogError("Haste Skill UI reference is NULL for the owner!");
           }*/
    }

    private void OnIsCrouchingChanged(bool previousValue, bool newValue)
    {
        if (animator != null)
        {
            animator.SetBool("IsCrouching", newValue);
        }
    }
    private void OnIsGlidingChanged(bool previousValue, bool newValue)
    {
        if (animator != null)
        {
            animator.SetBool("IsGliding", newValue);
        }
    }
    [System.Obsolete]
    public void TakeDamage()
    {
        if (!IsServer || IsEliminated.Value) return;

        Health.Value--;

        if (Health.Value <= 0)
        {
            IsEliminated.Value = true;

            // --- THE NEW, SIMPLE LOGIC ---
            // Find the spectator spawn point in the scene.
            Transform spectatorPoint = GameObject.FindWithTag("SpectatorSpawn")?.transform;
            if (spectatorPoint != null)
            {
                // Tell the client who owns this player to teleport to the spectator area.
                TeleportToPositionClientRpc(spectatorPoint.position, spectatorPoint.rotation);
            }
        }
    }
    [ClientRpc]
    private void TeleportToPositionClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
    {
        // This runs on the specific client who needs to be teleported.
        if (!IsOwner) return;

        Debug.Log("Teleporting to a new position.");

        // We still use the coroutine to ensure a clean, physics-safe teleport.
        StartCoroutine(TeleportAndReenable(position, rotation));
    }


    // --- Update SceneLoaded to re-enable the controller ---

    private void OnHealthChanged(int previousValue, int newValue)
    {
        // You could update a health bar UI here if you wanted.
        Debug.Log($"My health changed to: {newValue}");
    }

    private void OnIsPushingChanged(bool previousValue, bool newValue)
    {
        // This function runs for everyone and sets the animation state.
        if (animator != null)
        {
            animator.SetBool("IsPushing", newValue);
        }
    }
    private void OnHatChanged(int previousValue, int newValue)
    {
        EquipCosmetic(hatsContainer, newValue);
    }
    private void OnPantsChanged(int previousValue, int newValue)
    {
        EquipCosmetic(pantsContainer, newValue);
    }
    private void OnShoesChanged(int previousValue, int newValue)
    {
        EquipCosmetic(shoesContainer, newValue);
    }
    private void OnTshirtChanged(int previousValue, int newValue)
    {
        EquipCosmetic(tshirtsContainer, newValue);
    }
    private void OnCostumeChanged(int previousValue, int newValue)
    {
        EquipCosmetic(costumesContainer, newValue);
    }
    private void OnGlassesChanged(int previousValue, int newValue)
    {
        EquipCosmetic(glassesContainer, newValue);
    }
    private void OnFacesChanged(int previousValue, int newValue)
    {
        EquipCosmetic(facesContainer, newValue);
    }
    private void OnGlovesChanged(int previousValue, int newValue)
    {
        EquipCosmetic(glovesContainer, newValue);
    }

    void EquipCosmetic(Transform container, int activeIndex)
    {
        // First, disable all items in the container.
        for (int i = 0; i < container.childCount; i++)
        {
            container.GetChild(i).gameObject.SetActive(false);
        }

        // Now, if the index is valid, enable only the chosen one.
        if (activeIndex >= 0 && activeIndex < container.childCount)
        {
            container.GetChild(activeIndex).gameObject.SetActive(true);
        }
    }

    /* [System.Obsolete]
     private void OnCollisionStay(Collision collision)
     {
         if (!IsOwner) return;

         if (collision.gameObject.TryGetComponent<PushableBox>(out PushableBox box))
         {
             float horizontalInput = Input.GetAxis("Horizontal");
             bool isTryingToPush = Mathf.Abs(horizontalInput) > 0.1f;

             // The owner's only job is to update the master switch (the NetworkVariable) if the state changes.
             if (network_isPushing.Value != isTryingToPush)
             {
                 network_isPushing.Value = isTryingToPush;
             }

             // The RPC calls to physically move the box are still sent based on direct input.
             if (isTryingToPush)
             {
                 RequestPushServerRpc(box.NetworkObjectId, new Vector3(horizontalInput, 0, 0));
             }
             else
             {
                 RequestStopPushServerRpc(box.NetworkObjectId);
             }
         }
     }*/

    [ServerRpc]
    public void RequestCosmeticChangeServerRpc(int newItemIndex, CosmeticType type)
    {
        if (type == CosmeticType.Hat) HatIndex.Value = newItemIndex;
        else if (type == CosmeticType.Pants) PantsIndex.Value = newItemIndex;
        else if (type == CosmeticType.Shoes) ShoesIndex.Value = newItemIndex;
        else if (type == CosmeticType.Tshirt) TshirtIndex.Value = newItemIndex;
        else if (type == CosmeticType.Costume) CostumeIndex.Value = newItemIndex;
        else if (type == CosmeticType.Glasses) GlassesIndex.Value = newItemIndex;
        else if (type == CosmeticType.Face) FacesIndex.Value = newItemIndex;
        else if (type == CosmeticType.Gloves) GlovesIndex.Value = newItemIndex;
    }


    private void OnRoleChanged(PlayerRole previousRole, PlayerRole newRole)
    {
        if (characterRenderer == null) return;

        if (newRole == PlayerRole.Survivor)
        {
            characterRenderer.material.color = Color.white;
        }
        else // If Infected or AlphaInfected
        {
            characterRenderer.material.color = Color.gray;
        }
        if (previousRole == PlayerRole.Survivor && newRole != PlayerRole.Survivor)
        {
            // If so, start the special "transformation" coroutine.
            StartCoroutine(BecomeZombieSequence());
        }
        else
        {
            // If we are changing in any other way (e.g., zombie to survivor),
            // just update the layer weight instantly.
            UpdateZombieLayerWeight(newRole != PlayerRole.Survivor);
        }
    }
    // The final, correct version of the coroutine.

    private IEnumerator BecomeZombieSequence()
    {
        // Find the layer index.
        int zombieLayerIndex = animator.GetLayerIndex("ZombieOverride");
        if (zombieLayerIndex == -1)
        {
            Debug.LogError("Could not find the 'ZombieOverride' layer in the Animator!");
            yield break; // Exit the coroutine if the layer doesn't exist.
        }

        // Step 1: Force the Base Layer to be active by turning the override off.
        animator.SetLayerWeight(zombieLayerIndex, 0f);

        // Step 2: Force the "GetInfected" animation to play.
        string getInfectedStateName = "GetInfected"; // Use a variable for the name
        animator.Play(getInfectedStateName, 0);

        // Tell the server to sync this animation for other players.
        if (IsOwner)
        {
            RequestPlayAnimationServerRpc(getInfectedStateName);
        }

        // --- Step 3: Wait for the animation to finish (THE CORRECTED CODE) ---
        float getInfectedDuration = 0f;

        // Search through all animation clips in the animator controller to find the right one.
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            // Check if the clip's name matches the state we just played.
            if (clip.name == getInfectedStateName)
            {
                // If we found it, get its length in seconds.
                getInfectedDuration = clip.length;
                break; // Exit the loop since we found what we need.
            }
        }

        if (getInfectedDuration == 0f)
        {
            Debug.LogWarning($"Could not find animation clip named '{getInfectedStateName}' to get its duration. Waiting for a default time.");
            getInfectedDuration = 6.8f; // A safe fallback wait time.
        }

        // Wait for the full duration of the animation.
        yield return new WaitForSeconds(getInfectedDuration + 0.1f);

        // --- Step 4: Now that the transformation is complete, turn the ZombieOverride layer ON ---
        animator.SetLayerWeight(zombieLayerIndex, 1f);
    }
    [ServerRpc]
    private void RequestPlayAnimationServerRpc(string stateName)
    {
        PlayAnimationClientRpc(stateName);
    }
    [ClientRpc]
    private void PlayAnimationClientRpc(string stateName)
    {
        // The owner has already played their own animation.
        // This command is for all the remote observers.
        if (!IsOwner && animator != null)
        {
            animator.Play(stateName, 0); // Play the specified state on layer 0
        }
    }
    private void UpdateZombieLayerWeight(bool isZombie)
    {
        if (animator == null) return;

        int zombieLayerIndex = animator.GetLayerIndex("ZombieOverride");
        if (zombieLayerIndex != -1)
        {
            // If isZombie is true, weight is 1. If false, weight is 0.
            animator.SetLayerWeight(zombieLayerIndex, isZombie ? 1f : 0f);
        }
    }
    /*  [ServerRpc]
      private void RequestGetInfectedAnimationServerRpc()
      {
          // This code now runs on the SERVER.
          // The server tells its authoritative version of the animator to fire the trigger.
          if (animator != null)
          {
              animator.SetTrigger("GetInfected");
          }

          // The NetworkAnimator component will see this trigger on the server
          // and automatically replicate it to all clients, including the Host.
      }*/
    [System.Obsolete]
    private void SceneLoaded(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        // Your existing logic here to reset health, etc., is still good.
        if (IsServer)
        {
            Health.Value = maxHealth;
            IsEliminated.Value = false;
        }
        if (IsOwner)
        {
            // Tell the server to reset our role (which fixes the color).
            RequestRoleResetServerRpc();

            // We can also ask the server to reset our health here.

        }

        // --- THE FIX ---
        // When a new scene loads, EVERYONE needs to re-enable their controller.
        if (IsOwner)
        {
            this.enabled = true;
        }

        // The rest of the function (respawn, camera reset) is still correct.
        Respawn();
        cameraIsSetUp = false;
        hasWonThisRound = false;
        hasteSkillUI = null;
        if (sceneName == "GameScene3")
        {
            // If we are, try to find the UIManager and get the reference.
            if (IsOwner && UIManager.Instance != null)
            {
                hasteSkillUI = UIManager.Instance.HasteSkillUI;
                Debug.Log("Haste Skill UI reference acquired for Zombie Scene!");
                // Re-run the show/hide logic now that we have the reference.
                HandleRoleForSkillUI(Role.Value, Role.Value);
            }
        }
    }
    [ServerRpc]
    private void RequestRoleResetServerRpc()
    {
        // The server sets our role back to the default.
        Role.Value = PlayerRole.Survivor;
    }
    [System.Obsolete]
    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
        }
    }
    [System.Obsolete]
    private void Update()
    {
        // if (IsEliminated.Value) return;
        if (controlsAreLocked) return;
        if (!IsOwner) return;
        if (IsOwner && !cameraIsSetUp)
        {
            // SetupCamera();
        }

        if (IsOwner)
        {

            // The owner calculates their current speed based on their Rigidbody's velocity.
            float currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

            // The owner updates the NetworkVariable. This change is automatically sent to the server.
            networkAnimatorSpeed.Value = currentSpeed;

            // The owner checks for input and requests to dance via an RPC.
            if (Input.GetKeyDown(KeyCode.B))
            {
                animator.SetTrigger("Dance");
                RequestDanceServerRpc();
            }
            // --- RUN LOGIC ---
            // Check if the Left Shift key is being held down.
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            animator.SetBool("IsRunning", isRunning); // Send this to the animator

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // First, check for a normal ground jump.
                if (IsGrounded())
                {
                    // This is our existing, working ground jump logic.
                    if (!IsServer)
                    {
                        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                        animator.SetTrigger("Jump");
                    }
                    RequestJumpServerRpc();
                }
                // --- NEW WALL JUMP LOGIC ---
                else // If we are NOT grounded, check if we can wall jump.
                {
                    if (CanWallJump(out Vector3 wallNormal))
                    {
                        // If the check passes, perform the wall jump.
                        if (!IsServer)
                        {
                            // Client predicts the wall jump.

                            PerformWallJump(wallNormal);
                        }
                        // Tell the server to perform the authoritative wall jump.
                        RequestWallJumpServerRpc(wallNormal);
                    }
                }
            }
            bool isCrouchHeld = Input.GetKey(KeyCode.C);
            if (network_isCrouching.Value != isCrouchHeld)
            {
                network_isCrouching.Value = isCrouchHeld;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                if (!IsServer) animator.SetTrigger("Dance"); // Predict for clients only
                RequestDanceServerRpc();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                if (!IsServer) animator.SetTrigger("NewEmote"); // Predict for clients only
                RequestNewEmoteServerRpc();
            } // RPC for the new emote
        }
        animator.SetFloat("Speed", networkAnimatorSpeed.Value);

        if (hasteCooldownTimer > 0)
        {
            hasteCooldownTimer -= Time.deltaTime;
            // Update the UI visual.
            if (hasteSkillUI != null)
            {
                hasteSkillUI.UpdateCooldown(hasteCooldownTimer, hasteCooldown);
            }
        }

        // Check if we can trigger the skill.
        // We must be a zombie, not already hasted, and off cooldown.
        if (Role.Value != PlayerRole.Survivor && !isHasteActive && hasteCooldownTimer <= 0)
        {
            // And there must be at least one survivor in our detection zone.
            if (predatorZone != null && predatorZone.SurvivorsInZone.Count > 0)
            {
                // Trigger the skill!
                RequestHasteServerRpc();
            }
        }
    }
    /* private void InitializeUI()
     {
         // Try to find the UI object in the scene.
         GameObject uiGO = GameObject.FindWithTag("HasteSkillUI");
         if (uiGO != null)
         {
             // If we found it, get the component and set our flag to true.
             hasteSkillUI = uiGO.GetComponent<HasteSkillUIController>();
             uiInitialized = true; // Success! We won't search anymore.

             // Now that we've found it, re-run the logic to make sure it's shown/hidden correctly.
             HandleRoleForSkillUI(Role.Value, Role.Value);
             Debug.Log("Haste Skill UI Initialized Successfully!");
         }
     }*/
    // If we don't find it, we do nothing. The Update loop will try again next frame.

    private bool CanWallJump(out Vector3 wallNormal)
    {
        // Default the normal to zero.
        wallNormal = Vector3.zero;

        // Use a raycast to detect a wall in front of us.
        // We use the character's forward direction for the ray.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallDetectionDistance, whatIsWall))
        {
            // We hit a valid wall!
            // The "normal" is a vector that points directly away from the surface we hit.
            wallNormal = hit.normal;
            return true;
        }

        return false;
    }

    [System.Obsolete]
    private void PerformWallJump(Vector3 wallNormal)
    {
        // The jump direction is a combination of upwards and away from the wall.
        Vector3 jumpDirection = (Vector3.up + wallNormal).normalized;

        // Apply the force. We cancel out any previous velocity for a snappy feel.
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(jumpDirection * wallJumpForce, ForceMode.Impulse);

        // Play the jump animation.
        animator.SetTrigger("Jump");

        // Instantly flip the character to face away from the wall.
        transform.rotation = Quaternion.LookRotation(-wallNormal);
    }
    [ServerRpc]
    [System.Obsolete]
    private void RequestWallJumpServerRpc(Vector3 wallNormal)
    {
        // The server validates the action and applies the authoritative physics.
        // A more secure version would re-do the raycast here to prevent cheating. For now, this is fine.

        Vector3 jumpDirection = (Vector3.up + wallNormal).normalized;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(jumpDirection * wallJumpForce, ForceMode.Impulse);
        animator.SetTrigger("Jump");
        WallJumpRotationClientRpc(wallNormal);
        // Tell all clients to play the animation and rotate the character.
        // WallJumpVisualsClientRpc(wallNormal);
    }
    [ClientRpc]
    private void WallJumpRotationClientRpc(Vector3 wallNormal)
    {
        // Remote clients (who are not the owner) need to be told to rotate.
        // The owner has already predicted their own rotation.
        if (!IsOwner)
        {
            transform.rotation = Quaternion.LookRotation(-wallNormal);
        }
    }
    /* [ClientRpc]
     private void WallJumpVisualsClientRpc(Vector3 wallNormal)
     {
         // We only want remote clients to execute this. The owner already predicted it.
         if (!IsOwner)
         {
             animator.SetTrigger("Jump");
             transform.rotation = Quaternion.LookRotation(-wallNormal);
         }
     }*/
    public bool IsGrounded()
    {
        float distanceToGround = 1.1f;
        return Physics.Raycast(transform.position, Vector3.down, distanceToGround);
    }

    [ServerRpc]
    private void RequestDanceServerRpc()
    {
        // This code runs on the server.
        // It tells the animator on the server's version of this player to dance.
        // The NetworkAnimator will then sync this trigger to all clients.
        animator.SetTrigger("Dance");
    }

    // We use OnTriggerEnter to detect collisions with our special zones.
    [System.Obsolete]
    // Inside PlayerController.cs
    private void OnCollisionEnter(Collision collision)
    {
        // Only the owner should try to initiate an infection.
        if (!IsOwner) return;

        // Check if we collided with another player.
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController otherPlayer))
        {
            // If I am infected and the other player is a survivor...
            if (Role.Value != PlayerRole.Survivor && otherPlayer.Role.Value == PlayerRole.Survivor)
            {
                if (animator != null)
                {
                    animator.SetTrigger("ZombieAttack");
                }
                // ...tell the server to infect them!
                RequestInfectServerRpc(otherPlayer.NetworkObjectId);
            }
        }
    }

    /*[System.Obsolete]
    private void OnCollisionExit(Collision collision)
    {
        if (!IsOwner) return;

        if (collision.gameObject.TryGetComponent<PushableBox>(out PushableBox box))
        {
            // When we leave the box, ensure the state is set to false.
            network_isPushing.Value = false;

            // Also ensure we tell the server to stop the box's momentum.
            RequestStopPushServerRpc(box.NetworkObjectId);
        }
    }*/
    [ServerRpc]
    [System.Obsolete]
    private void RequestStopPushServerRpc(ulong boxNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(boxNetworkId, out NetworkObject boxObject))
        {
            PushableBox box = boxObject.GetComponent<PushableBox>();
            if (box != null)
            {
                box.StopPushing();
            }
        }
    }

    [System.Obsolete]
    private void OnTriggerEnter(Collider other)
    {
        // This is our detective's log. It will run on EVERY machine that detects a collision.
        Debug.Log($"Collision Detected on machine: {(IsServer ? "SERVER" : "CLIENT")}. Player Object: {gameObject.name}. Collided with: {other.gameObject.name}");

        // --- The rest of the logic remains the same ---
        if (!IsOwner)
        {
            return;
        }

        if (other.CompareTag("DeathZone"))
        {
            Debug.Log("SERVER is processing DeathZone collision.");
            Respawn();
        }

        if (other.CompareTag("EndPoint"))
        {
            // THE FIX: Add a check for our new flag.
            if (hasWonThisRound == false)
            {
                // Set the flag to true immediately so this can't be triggered again.
                hasWonThisRound = true;

                // Now, request the win.
                RequestWinServerRpc();
            }
        }
    }
    [ServerRpc]
    private void RequestInfectServerRpc(ulong targetNetworkObjectId)
    {
        // This runs on the server. Find the target player object.
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetObject))
        {
            PlayerController targetPlayer = targetObject.GetComponent<PlayerController>();

            // Double check the roles on the server to prevent cheating.
            if (Role.Value != PlayerRole.Survivor && targetPlayer.Role.Value == PlayerRole.Survivor)
            {
                // Change the target's role. This will automatically update their color for everyone.
                targetPlayer.Role.Value = PlayerRole.Infected;
                PlayZombieAttackAnimationClientRpc();
            }
        }
    }
    [ClientRpc]
    private void PlayZombieAttackAnimationClientRpc()
    {
        // This command runs on all clients.
        // We only want REMOTE players to play this, as the owner already predicted it.
        if (!IsOwner && animator != null)
        {
            animator.SetTrigger("ZombieAttack");
        }
    }
    [ServerRpc]
    [System.Obsolete]
    private void RequestWinServerRpc()
    {
        // Find the GameManager in the scene.
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // Tell the GameManager that THIS player (identified by their OwnerClientId) has won.
            gameManager.PlayerReachedEndPoint(OwnerClientId);
        }
    }


    [System.Obsolete]
    public void Respawn()
    {
        // This code is now only ever called on the server.
        Transform spawnPoint = GameObject.Find("SpawnPoint").transform;
        if (spawnPoint != null)
        {
            rb.position = spawnPoint.position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        RespawnClientRpc(spawnPoint.position, spawnPoint.rotation);
    }
    [ClientRpc]
    private void RespawnClientRpc(Vector3 spawnPosition, Quaternion spawnRotation, ClientRpcParams rpcParams = default)
    {
        // This code now runs on the client's machine.

        // To ensure the client has authority to move their own object without fighting the server,
        // we should only run this if we are the owner.
        if (!IsOwner) return;

        // We use a coroutine to temporarily disable the character controller/rigidbody physics
        // to ensure a clean teleport.
        StartCoroutine(TeleportAndReenable(spawnPosition, spawnRotation));
    }

    private System.Collections.IEnumerator TeleportAndReenable(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        // Temporarily disable physics simulation to prevent weirdness
        rb.isKinematic = true;

        // Wait for the end of the physics frame to ensure all calculations are done.
        yield return new WaitForFixedUpdate();

        // Move the character
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        // Reset velocities
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Wait another frame before re-enabling physics.
        yield return new WaitForFixedUpdate();

        // Re-enable physics.
        rb.isKinematic = false;
        rb.WakeUp();
    }

    /*  [ServerRpc] // A client calls this, but it runs ON THE SERVER.
      private void RequestRespawnServerRpc()
      {
          // Find the spawn point in the scene.
          Transform spawnPoint = GameObject.Find("SpawnPoint").transform;

          if (spawnPoint != null)
          {
              // This is how you teleport a Rigidbody correctly.
              rb.position = spawnPoint.position;
              rb.linearVelocity = Vector3.zero; // Reset velocity to stop momentum.
              rb.angularVelocity = Vector3.zero;
          }
          else
          {
              Debug.LogError("Server could not find an object named 'SpawnPoint'!");
          }
      }
    */
    // ---- The rest of the script remains the same ----


    /* private void SetupCamera()
     {
         CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
         if (cameraFollow != null)
         {
             cameraFollow.target = this.transform;
             cameraIsSetUp = true;
         }
     }*/

    [System.Obsolete]
    private void FixedUpdate()
    {

        //if (IsEliminated.Value) return;
        if (network_isCrouching.Value)
        {
            // We can also ensure our Rigidbody comes to a stop.
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        if (controlsAreLocked) return;
        // --- THE MISSING LINE IS HERE ---
        if (isLaunchLocked) return;
        if (IsOwner && network_isGliding.Value)
        {
            // If we are currently gliding and we just hit the ground...
            if (IsGrounded())
            {
                // ...tell the server to stop the gliding state.
                RequestSetGlidingStateServerRpc(false);
            }
        }

        if (!IsOwner || network_isGliding.Value)
        {
            return;
        }


        CheckForLaunchPad();

        // --- Scene-Aware Input ---
        /*float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = 0f;
        if (SceneManager.GetActiveScene().name != "GameScene4_Puzzle")
        {
            verticalInput = Input.GetAxis("Vertical");
        }*/
        Vector3 moveDirection;
        if (SceneManager.GetActiveScene().name == "GameScene4_Puzzle")
        {
            // 2.5D PUZZLE CONTROLS (World-Relative)
            float horizontalInput = Input.GetAxis("Horizontal");
            moveDirection = new Vector3(horizontalInput, 0, 0);
        }
        else
        {
            // 3D FREELOOK CONTROLS (Camera-Relative)
            if (mainCameraTransform == null)
            {
                if (Camera.main != null) mainCameraTransform = Camera.main.transform;
                else return; // Can't move if there's no camera to reference
            }
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 camForward = mainCameraTransform.forward;
            Vector3 camRight = mainCameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;

            moveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;
        }
        //Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        bool isTryingToMove = moveDirection.magnitude > 0.1f;
        // --- Pushing Logic (Only relevant in the puzzle scene, but safe to run everywhere) ---
        bool isPushingThisFrame = false;
        // We now use our new 'isTryingToMove' variable here
        if (isTryingToMove && SceneManager.GetActiveScene().name == "GameScene4_Puzzle")
        {
            RaycastHit hit;
            Vector3 pushDirection = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
            if (Physics.Raycast(transform.position, pushDirection.normalized, out hit, pushRaycastDistance)) // Normalize the direction
            {
                if (hit.collider.CompareTag("Pushable") && hit.collider.TryGetComponent<PushableBox>(out lastPushedBox))
                {
                    isPushingThisFrame = true;
                    RequestPushServerRpc(lastPushedBox.NetworkObjectId, pushDirection);
                }
            }
        }

        // Stop pushing logic
        if (network_isPushing.Value && !isPushingThisFrame && lastPushedBox != null)
        {
            RequestStopPushServerRpc(lastPushedBox.NetworkObjectId);
        }

        // Update the master animation switch
        network_isPushing.Value = isPushingThisFrame;


        // --- Final Movement Calculation ---
        float baseSpeed = animator.GetBool("IsRunning") ? runSpeed : walkSpeed;
        float finalSpeed = isPushingThisFrame ? pushSpeed : baseSpeed;
        finalSpeed *= speedMultiplier;
        if (isHasteActive)
        {
            finalSpeed *= hasteSpeedMultiplier;
        }
        rb.linearVelocity = new Vector3(moveDirection.normalized.x * finalSpeed, rb.linearVelocity.y, moveDirection.normalized.z * finalSpeed);


        // --- Rotation and Animation ---
        if (isTryingToMove) // Use the bool here for a clean check
        {
            transform.rotation = Quaternion.LookRotation(moveDirection.normalized);
        }
        animator.SetFloat("Speed", isPushingThisFrame ? 0f : moveDirection.magnitude);
    }
    [ServerRpc]
    private void RequestHasteServerRpc()
    {
        // The server starts the coroutine that manages the skill's duration.
        // It's a coroutine because it happens over time.
        StartCoroutine(HasteSequence());
    }
    private IEnumerator HasteSequence()
    {
        // Tell all clients to activate the effect.
        ActivateHasteClientRpc(true);

        // Wait for the duration of the speed boost.
        yield return new WaitForSeconds(hasteDuration);

        // Tell all clients to deactivate the effect.
        ActivateHasteClientRpc(false);
    }
    [ClientRpc]
    private void ActivateHasteClientRpc(bool activate)
    {
        // This runs on all clients, but the logic inside only affects the owner.
        if (!IsOwner) return;

        isHasteActive = activate;

        if (hasteSkillUI != null)
        {
            hasteSkillUI.SetActiveState(activate);
        }
        if (activate)
        {
            Debug.Log("Haste skill ACTIVATED!");
            // You could add a particle effect here.
        }
        else
        {
            Debug.Log("Haste skill DEACTIVATED. Cooldown started.");
            // When the haste ends, start the cooldown timer.
            hasteCooldownTimer = hasteCooldown;
        }
    }
    [System.Obsolete]
    public void LockControls()
    {
        controlsAreLocked = true;

        // It's also a good idea to stop any current movement.
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            animator.SetFloat("Speed", 0);
        }

        Debug.Log("Player controls LOCKED.");
    }
    public void UnlockControls()
    {
        controlsAreLocked = false;
        Debug.Log("Player controls UNLOCKED.");
    }
    [System.Obsolete]
    private void CheckForLaunchPad()
    {
        // If we are on cooldown, don't do anything.
        if (onLaunchPadCooldown) return;

        // Shoot a short raycast straight down from the player's feet.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f))
        {
            // Did the raycast hit a launch pad?
            if (hit.collider.TryGetComponent<LaunchPad>(out LaunchPad pad))
            {
                // Yes! Start the cooldown and request a launch.
                onLaunchPadCooldown = true;
                Invoke(nameof(ResetLaunchCooldown), 1.5f); // Reset cooldown after 1.5 seconds

                // Get the launch details from the pad we hit.
                Vector3 direction = (pad.launchTarget.position - pad.transform.position).normalized;
                float force = pad.launchForce;

                // Tell the server to launch us.
                RequestLaunchServerRpc(direction, force);
            }
        }
    }
    private void ResetLaunchCooldown()
    {
        onLaunchPadCooldown = false;
    }
    [ServerRpc]
    [System.Obsolete]
    private void RequestLaunchServerRpc(Vector3 direction, float force)
    {
        // The server calls the public Launch function.
        Launch(direction, force);
    }
    [System.Obsolete]
    public void Launch(Vector3 direction, float force)
    {
        // This can only be executed by the server.
        if (!IsServer) return;
        Debug.Log($"SERVER: Player {OwnerClientId}'s Launch() function was called. Applying velocity: {direction * force}");
        // Tell the server to set the gliding state to true.
        LaunchPlayerClientRpc(direction, force, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        });

        // Apply the launch force.
        //rb.velocity = Vector3.zero; // Reset any existing velocity.
        // rb.velocity = direction * force;
        //StartCoroutine(LaunchLockout());

    }
    [ClientRpc]
    [System.Obsolete]
    private void LaunchPlayerClientRpc(Vector3 direction, float force, ClientRpcParams rpcParams = default)
    {
        // This now runs on the correct client's machine.
        if (!IsOwner) return;
        RequestSetGlidingStateServerRpc(true);
        //rb.velocity = direction * force;
        // network_isGliding.Value = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);
        // rb.velocity = direction * force;

        // Start the lockout to protect the velocity from FixedUpdate.
        StartCoroutine(LaunchLockout());
    }
    private IEnumerator LaunchLockout()
    {
        isLaunchLocked = true;
        yield return new WaitForSeconds(0.2f);
        isLaunchLocked = false;
    }
    [ServerRpc]
    private void RequestSetGlidingStateServerRpc(bool isGliding)
    {
        // The server authoritatively sets the gliding state.
        network_isGliding.Value = isGliding;
    }
    public void ApplySlowEffect(float duration)
    {
        // This needs to be an RPC so the server can tell the client to start the coroutine.
        ApplySlowClientRpc(duration);
    }
    [ClientRpc]
    private void ApplySlowClientRpc(float duration)
    {
        // This runs on the client that got hit.
        if (!IsOwner) return;

        // Start the coroutine that handles the slow effect over time.
        StartCoroutine(SlowCoroutine(duration));
    }
    private IEnumerator SlowCoroutine(float duration)
    {
        Debug.Log("I've been slowed!");
        speedMultiplier = 0.5f; // Set speed to 50% of normal

        // You could also change the player's color here to show they are slowed.

        yield return new WaitForSeconds(duration); // Wait for the slow to wear off

        speedMultiplier = 1.0f; // Return to normal speed
        Debug.Log("Slow effect has worn off.");
    }
    [ServerRpc]
    [System.Obsolete]
    private void RequestPushServerRpc(ulong boxNetworkId, Vector3 direction)
    {
        // This code runs on the SERVER.

        // Find the box that the client wants to push.
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(boxNetworkId, out NetworkObject boxObject))
        {
            PushableBox box = boxObject.GetComponent<PushableBox>();
            if (box != null)
            {
                // Call the public Push function on the box.
                // The box's own script will then handle moving it.
                box.Push(direction, pushSpeed);
            }
        }
    }
    [ServerRpc]
    private void RequestJumpServerRpc()
    {
        // The server's job is now purely for authority and syncing to OTHERS.
        // It still applies its own force to keep physics in check.
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // It still triggers the animation on its version of the animator.
        // The NetworkAnimator will send this to all OTHER clients (but the original client
        // will just ignore it since its animation is already playing).
        animator.SetTrigger("Jump");
    }

    [ServerRpc]
    private void RequestNewEmoteServerRpc()
    {
        animator.SetTrigger("NewEmote");
    }

    ///////////////////// PLATFORM ELIMINATION LOGIC ///////////////////////
    public void Eliminate()
    {
        if (!IsServer) return;

        if (!isEliminatedTepsi.Value)
        {
            Debug.Log($"Player {OwnerClientId} has been eliminated and is now a spectator.");
            isEliminatedTepsi.Value = true;
            EnterSpectatorModeClientRpc();
        }
        
    }

    [ClientRpc]
    private void EnterSpectatorModeClientRpc()
    {
        // Disable player movement or gravity
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Disable collider to prevent interactions
        if (col != null)
            col.enabled = false;

        // Optional: hide player model or UI

        // Switch to spectator camera
        if (IsOwner)
        {
            Debug.Log($"Player {OwnerClientId} entered spectator mode.");
            if (playerCamera != null) playerCamera.SetActive(false);
            if (spectatorCamera != null) spectatorCamera.SetActive(true);
        }

        
    }

    void Awake()
    {
        col = GetComponent<Collider>();
    }
}