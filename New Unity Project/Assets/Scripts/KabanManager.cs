using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KabanManager : MonoBehaviour
{
    #region Singletone

    public static KabanManager instance;

    void Awake()
    {
        instance = this;
    }

    #endregion

    public ISet<Kaban> kabans = new HashSet<Kaban>();
}