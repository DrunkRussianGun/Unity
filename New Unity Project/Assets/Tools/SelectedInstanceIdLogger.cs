using UnityEditor;
using UnityEngine;

namespace Tools
{
	[ExecuteInEditMode]
	public class SelectedInstanceIdLogger : MonoBehaviour
	{
		#region Singleton

		private static volatile bool isInitialized;

		private static readonly object synchronizationLock = new object(); 

		#endregion

		public void OnEnable()
		{
			if (isInitialized)
				return;
			
			lock (synchronizationLock)
			{
				if (isInitialized)
					return;
				isInitialized = true;

				var oldCallback = Selection.selectionChanged;
				Selection.selectionChanged = () =>
				{
					oldCallback?.Invoke();
					OnSelectionChange();
				};
			}
		}

		private void OnSelectionChange()
		{
			if (Selection.instanceIDs.Length != 1)
				return;

			var selectedObject = Selection.activeObject;
			if (!selectedObject)
				return;

			Debug.Log($"Выбран объект {selectedObject.name} с ID {selectedObject.GetInstanceID()}");
		}
	}
}
