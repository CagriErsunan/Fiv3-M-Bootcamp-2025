using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSinCos : MonoBehaviour
{
    [SerializeField] float amplitudex, frequencyx, amplitudey, frequencyy, amplitudez, frequencyz;
    private Vector3 initialPos;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float x = initialPos.x + amplitudex * Mathf.Sin(Time.time * frequencyx);
        float y = initialPos.y + amplitudey * Mathf.Sin(Time.time * frequencyy);
        float z = initialPos.z + amplitudez * Mathf.Sin(Time.time * frequencyz);

        transform.localPosition = new Vector3( x, y, z);
    }
}
