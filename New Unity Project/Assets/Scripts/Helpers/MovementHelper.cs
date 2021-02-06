using System;
using UnityEngine;

public static class MovementHelper
{
	public static Quaternion GetTurn(this Transform transform, Vector3 direction, float maxAngleInDeg)
	{
		var forward = transform.forward;
		var turnAngle = Math.Min(maxAngleInDeg, Vector3.Angle(forward, direction));
		var turnAxis = Vector3.Cross(forward, direction);
		return Quaternion.AngleAxis(turnAngle, transform.InverseTransformVector(turnAxis));
	}
	
	public static Vector3 GetTorque(
		this Transform transform,
		Quaternion currentRotation,
		Vector3 direction,
		float maxAngleInDeg)
	{
		var turn = transform.GetTurn(direction, maxAngleInDeg);
		// https://stackoverflow.com/questions/24216507/how-to-calculate-euler-angles-from-forward-up-and-right-vectors/24225689#24225689
		var magicQuaternion = currentRotation * turn * Quaternion.Inverse(currentRotation);
		return new Vector3(magicQuaternion.x, magicQuaternion.y, magicQuaternion.z);
	}

	public static Vector3 GetAcceleratingVelocity(
		this Transform transform,
		Vector3 currentVelocity,
		float velocityToAdd,
		float maxVelocity)
	{
		if (currentVelocity.magnitude >= maxVelocity)
			return Vector3.zero;

		var forward = transform.forward;
		var angleBetweenCurrentAndAddedVelocitiesInRad = Vector3.Angle(currentVelocity, forward) * Mathf.Deg2Rad;
		var angleBetweenCurrentAndAddedVelocitiesSin = Math.Sin(angleBetweenCurrentAndAddedVelocitiesInRad);
		return forward
			* Mathf.Min(
				velocityToAdd,
				(float)(Math.Sqrt(
						maxVelocity * maxVelocity
					   - currentVelocity.magnitude * currentVelocity.magnitude
							* angleBetweenCurrentAndAddedVelocitiesSin * angleBetweenCurrentAndAddedVelocitiesSin)
					- currentVelocity.magnitude * Math.Cos(angleBetweenCurrentAndAddedVelocitiesInRad)));
	}
}