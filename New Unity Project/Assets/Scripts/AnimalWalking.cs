using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalWalking : MonoBehaviour
{
    public float speed;
    public Vector3 direction;

    void FixedUpdate()
    {
        transform.Translate(speed*direction*Time.deltaTime, Space.World);
    }
}
