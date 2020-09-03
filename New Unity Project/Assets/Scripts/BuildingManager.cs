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

    public ISet<GameObject> buildings = new HashSet<GameObject>();
}
