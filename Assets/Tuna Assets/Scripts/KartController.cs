using UnityEngine;
using Unity.Netcode;
using NUnit;
using Kart.Items;

namespace Kart
{
    public class KartController : NetworkBehaviour
    {
        public float acceleration = 15f;
        public float maxSpeed = 25f;
        public float turnSpeed = 120f;
        KartInventory inv;
        private Rigidbody rb;
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // Basit test için þimdilik yukarý koy
                transform.position = new Vector3(0, 2, 0);
                transform.rotation = Quaternion.identity;
            }
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            inv = GetComponent<KartInventory>();
        }

        void FixedUpdate()
        {
            if (!IsOwner) return;

            float moveInput = Input.GetAxis("Vertical");
            float turnInput = Input.GetAxis("Horizontal");

            if (rb.linearVelocity.magnitude < maxSpeed)
                rb.AddForce(transform.forward * moveInput * acceleration, ForceMode.Acceleration);

            float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                inv.UseItem();
            }
        }


    }
}
