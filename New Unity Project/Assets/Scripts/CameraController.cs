﻿using UnityEngine;

[AddComponentMenu("Camera-Control/Camera with zoom")]
public class CameraController : MonoBehaviour
{
	public Transform target;
	public float xSpeed;
	public float xSpeedDistanceFactor;
	public float ySpeed;
	public float zSpeed;

	public float yMinLimit;
	public float yMaxLimit;

	public float distance;
	public float distanceMin;
	public float distanceMax;

	public float distanceFromObstacles;
	public LayerMask obstacleLayers;

	private Rigidbody rigidbody;

	private float x;
	private float y;

	// Use this for initialization
	void Start() 
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

		rigidbody = GetComponent<Rigidbody>();
		// Make the rigid body not change rotation
		if (rigidbody != null)
			rigidbody.freezeRotation = true;
	}

	void LateUpdate()
	{
		if (!target)
			return;

		if (Input.GetMouseButton(1)) 
		{
			x += Input.GetAxis("Mouse X") * xSpeed * Mathf.Pow(distance, xSpeedDistanceFactor);
			y -= Input.GetAxis("Mouse Y") * ySpeed;
		}

		y = ClampAngle(y, yMinLimit, yMaxLimit);

		if (Input.GetAxis("Mouse ScrollWheel") != 0)
		{
			distance -= Input.GetAxis("Mouse ScrollWheel") * zSpeed;
			distance = Mathf.Clamp(distance, distanceMin, distanceMax);
		}

		var targetPosition = target.position;
		var rotation = Quaternion.Euler(y, x, 0f);
		var negativeDistance = new Vector3(0f, 0f, -distance);
		var position = targetPosition + rotation * negativeDistance;

		var obstacleHit = GetObstacleHit(targetPosition, position);
		if (obstacleHit != null)
		{
			obstacleHit.Value.normal.Scale(new Vector3(1, 1, 0));
			position = obstacleHit.Value.point + obstacleHit.Value.normal * distanceFromObstacles;
		}

		transform.rotation = rotation;
		transform.position = position;
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
			angle += 360f;
		if (angle > 360f)
			angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}

	private RaycastHit? GetObstacleHit(Vector3 targetPosition, Vector3 cameraPosition)
	{
		const int maxHitCount = 10;
		var currentHitCount = 0;

		var direction = (cameraPosition - targetPosition).normalized;
		var currentPosition = targetPosition;
		while (Physics.Linecast(currentPosition, cameraPosition, out var obstacleHit, obstacleLayers)
		       && currentHitCount < maxHitCount)
		{
			if (obstacleHit.collider.bounds.Contains(cameraPosition))
				return obstacleHit;

			currentPosition = obstacleHit.point + direction * distanceFromObstacles;
			++currentHitCount;
		}

		// ReSharper disable once InvertIf
		if (currentHitCount == maxHitCount)
		{
			Physics.Linecast(targetPosition, cameraPosition, out var firstObstacleHit, obstacleLayers);
			return firstObstacleHit;
		}

		return null;
	}
}