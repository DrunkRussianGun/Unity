using System.Collections.Generic;
using UnityEngine;

public class KabanManager : MonoBehaviour
{
    #region Singletone

    public static KabanManager Instance;

    void Awake()
    {
        Instance = this;
    }

    #endregion

    public ISet<Kaban> Kabans = new HashSet<Kaban>();
}