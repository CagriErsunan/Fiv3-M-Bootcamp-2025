using UnityEngine;

namespace Kart.Items
{
    public class RoketController : MonoBehaviour
    {
        public float speed = 15f;
        public float rotateSpeed = 200f;
        public float lifetime = 5f;

        private GameObject owner;
        private GameObject target;

        public void Initialize(GameObject ownerKart)
        {
            owner = ownerKart;
            target = FindClosestTarget();
            Destroy(gameObject, lifetime);
        }

        void FixedUpdate()
        {
            if (target == null)
            {
                transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);
                return;
            }

            Vector3 direction = (target.transform.position - transform.position).normalized;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotateSpeed * Time.fixedDeltaTime);
            transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);
        }

        GameObject FindClosestTarget()
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            GameObject closest = null;
            float minDist = Mathf.Infinity;

            foreach (var player in allPlayers)
            {
                if (player == owner) continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = player;
                }
            }

            return closest;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return;

            if (other.CompareTag("Player"))
            {
                Debug.Log("Roket çarptý: " + other.name);
                Destroy(gameObject);
            }
        }
    }
}
