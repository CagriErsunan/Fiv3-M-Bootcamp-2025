using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 3f;
    private bool isAlive = true;
    private bool eliminationNotified = false;

    private void Update()
    {
        if (!IsOwner || !isAlive) return;

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        float horizontal = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * horizontal * turnSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner || !isAlive || eliminationNotified) return;

        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("[PlayerMovement] Collision with Wall detected. Player eliminated.");
            isAlive = false;
            eliminationNotified = true;
            moveSpeed = 0f;

            StartCoroutine(TryNotifyElimination(NetworkManager.Singleton.LocalClientId));
        }
    }

    private IEnumerator TryNotifyElimination(ulong playerId)
    {
        float timeout = 3f;
        float timer = 0f;

        while (GameManager.Instance == null && timer < timeout)
        {
            Debug.Log("[PlayerMovement] Waiting for GameManager instance...");
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        if (GameManager.Instance != null)
        {
            Debug.Log("[PlayerMovement] GameManager found. Notifying elimination.");
            NotifyEliminatedServerRpc(playerId);
        }
        else
        {
            Debug.LogError("[PlayerMovement] GameManager instance NOT found after waiting. Cannot notify elimination.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyEliminatedServerRpc(ulong playerId)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[PlayerMovement] NotifyEliminatedServerRpc called but not on server.");
            return;
        }

        if (GameManager.Instance != null)
        {
            Debug.Log($"[PlayerMovement] NotifyEliminatedServerRpc called for playerId {playerId}");
            GameManager.Instance.NotifyPlayerEliminatedServerRpc(playerId);
        }
        else
        {
            Debug.LogError("[PlayerMovement] GameManager instance null in NotifyEliminatedServerRpc.");
        }
    }
}
