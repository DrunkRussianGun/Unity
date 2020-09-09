using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    #region Singletone

    public static BuildingManager instance;

    void Awake()
    {
        instance = this;
    }

    #endregion

    public ISet<Building> buildings = new HashSet<Building>();
	public float pushingAwayForceOnEnter = 20f;
	public float pushingAwayForceOnStay = 2f;
}
