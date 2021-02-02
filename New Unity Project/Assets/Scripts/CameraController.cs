using UnityEngine;

[AddComponentMenu("Camera-Control/Camera with zoom")]
public class CameraController : MonoBehaviour
{
	public Transform target;
	public float xSpeed;
	public float ySpeed;

	public float yMinLimit;
	public float yMaxLimit;

	public float distance;
	public float distanceMin;
	public float distanceMax;

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
			x += Input.GetAxis("Mouse X") * xSpeed * distance;
			y -= Input.GetAxis("Mouse Y") * ySpeed;
		}

		y = ClampAngle(y, yMinLimit, yMaxLimit);

		if (Input.GetAxis("Mouse ScrollWheel") != 0)
		{
			distance -= Input.GetAxis("Mouse ScrollWheel") * 5;
			distance = Mathf.Clamp(distance, distanceMin, distanceMax);
		}

		var rotation = Quaternion.Euler(y, x, 0f);
		var negativeDistance = new Vector3(0f, 0f, -distance);
		var position = target.position + rotation * negativeDistance;

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
}