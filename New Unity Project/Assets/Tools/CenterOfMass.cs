using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Tools
{
	[ExecuteInEditMode]
	public class CenterOfMass : MonoBehaviour
	{
		public Transform centerOfMass;

		[SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
		public void OnEnable()
		{
			GetComponent<Rigidbody>().centerOfMass = Vector3.Scale(centerOfMass.localPosition, transform.localScale);
		}

		#if UNITY_EDITOR
		public void Update()
		{
			// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
			OnEnable();
		}
		#endif

		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(GetComponent<Rigidbody>().worldCenterOfMass, 0.1f);
		}
	}
}