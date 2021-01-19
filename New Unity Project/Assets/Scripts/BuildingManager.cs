using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    #region Singletone

    public static BuildingManager Instance;

    void Awake()
    {
        Instance = this;
    }

    #endregion

    public ISet<Building> Buildings = new HashSet<Building>();
	public float pushingAwayForceOnEnter = 20f;
	public float pushingAwayForceOnStay = 2f;
}
