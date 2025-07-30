using UnityEngine;
using Unity.Netcode;

public class PushableBox : NetworkBehaviour
{
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
    }

    [System.Obsolete]
    public void Push(Vector3 direction, float speed)
    {
        if (!IsServer || rb == null) return;
        rb.velocity = new Vector3(direction.x * speed, rb.velocity.y, 0);
    }

    [System.Obsolete]
    public void StopPushing()
    {
        if (!IsServer || rb == null) return;
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
}