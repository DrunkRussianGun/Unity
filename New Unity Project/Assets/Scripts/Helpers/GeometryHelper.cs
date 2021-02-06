using UnityEngine;

public static class GeometryHelper
{
	public static Vector3 GetNormalInPlaneWith(this Vector3 vector, Vector3 vectorInPlane)
	{
		var velocityNormal = Vector3.Cross(vector, vectorInPlane);
		velocityNormal = Vector3.Cross(vector, velocityNormal);
		return -velocityNormal;
	}
}
