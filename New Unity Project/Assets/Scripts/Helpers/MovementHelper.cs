using System;
using UnityEngine;

namespace Helpers
{
	public static class MovementHelper
	{
		public static Quaternion GetRotation(
			this Transform transform,
			Vector3 from,
			Vector3 to,
			float maxAngleInDeg = Mathf.Infinity)
		{
			var turnAngle = Math.Min(maxAngleInDeg, Vector3.Angle(from, to));
			var turnAxis = Vector3.Cross(from, to);
			return Quaternion.AngleAxis(turnAngle, transform.InverseTransformVector(turnAxis));
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
}