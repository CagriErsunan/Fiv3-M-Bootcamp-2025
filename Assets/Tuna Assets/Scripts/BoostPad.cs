// Scripts/BoostPad.cs
using UnityEngine;
using Unity.Netcode;

namespace Kart
{
    [RequireComponent(typeof(Collider))]
    public class BoostPad : NetworkBehaviour
    {
        public float boostForce = 30f;

        private void OnTriggerEnter(Collider other)
        {
            // Sadece sunucu fizik uygular
            if (!IsServer) return;

            // Kart mý çarptý?
            var rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 boostDirection = transform.forward;
                rb.AddForce(boostDirection * boostForce, ForceMode.VelocityChange);
            }
        }
    }
}
