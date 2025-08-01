using UnityEngine;

public class WhaleOrbitMotion : MonoBehaviour
{
    public float rotationSpeed = 50f;    
    public float orbitSpeed = 1f;        
    public float orbitRadius = 1f;      

    private Vector3 centerPosition;
    private float angle;

    void Start()
    {
        centerPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        angle += orbitSpeed * Time.deltaTime;
        float x = Mathf.Cos(angle) * orbitRadius;
        float z = Mathf.Sin(angle) * orbitRadius;
        transform.position = centerPosition + new Vector3(x, 0, z);
    }
}
