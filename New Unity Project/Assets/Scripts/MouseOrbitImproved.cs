﻿using UnityEngine;


[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour {
 
    public Transform target;
    public float distance = 12.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
 
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
 
    public float distanceMin = .5f;
    public float distanceMax = 15f;
 
    private Rigidbody rigidbody;
 
    private float x = 0.0f;
    private float y = 0.0f;
 
    // Use this for initialization
    void Start() 
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
 
        rigidbody = GetComponent<Rigidbody>();
 
        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }
    }
 
    void LateUpdate() 
    {
        if (target) 
        {
        	if (Input.GetMouseButton(1)) 
        	{
        		x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            	y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        	}
            
            y = ClampAngle(y, yMinLimit, yMaxLimit);

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                distance = distance - Input.GetAxis("Mouse ScrollWheel") * 5;
                if (distance < distanceMin) { distance = distanceMin; }
                if (distance > distanceMax) { distance = distanceMax; }
            }
 
            var rotation = Quaternion.Euler(y, x, 0);
            
            var negDistance = new Vector3(0.0f, 0.0f, -distance);
            var position = rotation * negDistance + target.position;
 
            transform.rotation = rotation;
            transform.position = position;
        }
    }
 
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}