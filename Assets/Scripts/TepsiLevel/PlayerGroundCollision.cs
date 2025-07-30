using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerGroundCollision : NetworkBehaviour
{
    [SerializeField] private string groundTag = "Zemin";

    private bool isEliminated = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || isEliminated) return;

        if (collision.gameObject.CompareTag(groundTag))
        {
            isEliminated = true;

            GameManager.Instance.PlayerDied(OwnerClientId);

            SetSpectatorModeClientRpc();
        }
    }

    [ClientRpc]
    private void SetSpectatorModeClientRpc()
    {
        // 1. Görünmez yap
        if (TryGetComponent(out MeshRenderer renderer))
            renderer.enabled = false;

        // 2. Çarpışmayı kapat
        if (TryGetComponent(out Collider collider))
            collider.enabled = false;

        // 3. Hareket scriptini kapat
        if (TryGetComponent(out TepsiCharacterController movement))
            movement.enabled = false;

        // 4. NetworkTransform'u durdur
        if (TryGetComponent(out NetworkTransform netTransform))
            netTransform.enabled = false;

        // 5. KAMERA? Oyuncunun kamerası aktif kalsın (istersen cinematic kamera yapabiliriz)
        Debug.Log("Oyuncu elendi ama sahneyi izlemeye devam ediyor.");
    }
}
