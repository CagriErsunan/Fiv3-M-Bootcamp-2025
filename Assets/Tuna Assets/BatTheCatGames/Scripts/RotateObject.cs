using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    
    public float rotationSpeed = 30.0f;
    public bool rotateOnX = true;
    public bool rotateOnY = true;
    public bool rotateOnZ = true;

    void Update()
    {
       
        float rotationAmount = rotationSpeed * Time.deltaTime;

    
        if (rotateOnX)
            transform.Rotate(Vector3.right, rotationAmount);
        if (rotateOnY)
            transform.Rotate(Vector3.up, rotationAmount);
        if (rotateOnZ)
            transform.Rotate(Vector3.forward, rotationAmount);
    }
}
