using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject go;

    // Update is called once per frame
    void Update()
    {
        if (Bank.money>=100 && Bank.food>=1000)
        {
            go.SetActive(true);
        }
    }
}
