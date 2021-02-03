using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class KabanManager : MonoBehaviour
{
	#region Singletone

	public static KabanManager Instance;

	void Awake()
	{
		WalkableAreasMask = walkableAreas
			.Select(areaName => 1 << NavMesh.GetAreaFromName(areaName))
			.Sum();

		Instance = this;
	}

	#endregion

	public readonly ISet<Kaban> Kabans = new HashSet<Kaban>();

	public string[] walkableAreas;

	[NonSerialized]
	public int WalkableAreasMask;
}