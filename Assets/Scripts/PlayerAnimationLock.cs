using UnityEngine;
using Unity.Netcode; // We need this to check for ownership

public class PlayerAnimationLock : StateMachineBehaviour
{
    // This function is called automatically the moment the animator ENTERS this state.
    [System.Obsolete]
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the PlayerController script on the root object.
        PlayerController playerController = animator.GetComponentInParent<PlayerController>();

        // We only want to lock controls for the player who owns this character.
        if (playerController != null && playerController.IsOwner)
        {
            // Call a public function on the controller to lock the controls.
            playerController.LockControls();
        }
    }

    // This function is called automatically the moment the animator EXITS this state.
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the PlayerController script again.
        PlayerController playerController = animator.GetComponentInParent<PlayerController>();

        if (playerController != null && playerController.IsOwner)
        {
            // Call a public function to unlock the controls.
            playerController.UnlockControls();
        }
    }
}