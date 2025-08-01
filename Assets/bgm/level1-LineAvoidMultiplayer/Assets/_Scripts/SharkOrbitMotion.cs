using UnityEngine;

public class SharkOrbitMotion : MonoBehaviour
{
    public float orbitRadius = 1f;
    public float orbitSpeed = 1f;
    public float rotationSpeed = 60f;

    private Vector3 startPosition;
    private float angle;

    void Start()
    {
        startPosition = transform.position;
        angle = Random.Range(0f, Mathf.PI * 2); // Hepsi aynı anda başlamasın diye
    }

    void Update()
    {
        // Yörünge hareketi
        angle += orbitSpeed * Time.deltaTime;
        float x = Mathf.Cos(angle) * orbitRadius;
        float z = Mathf.Sin(angle) * orbitRadius;
        transform.position = startPosition + new Vector3(x, 0, z);

        // Kendi etrafında dönme (yüzme efekti)
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
