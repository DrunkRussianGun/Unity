using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Vector2Int Size = Vector2Int.one;

    public static event Action<float> MoneyAdded;
    public static event Action<float> FoodAdded;
    public bool isFood;
    public GameObject canBeUpgradedTo;
    [SerializeField] private float moneyTimer;
    [SerializeField] private float moneyValue;
    [SerializeField] private float foodValue;

    private float timer;

    private void Update()
    {
        
        if (timer >= moneyTimer)
        {
            timer = 0;
            MoneyAdded.Invoke(moneyValue);
            if (isFood) FoodAdded.Invoke(foodValue);
        }

        timer += Time.deltaTime;
    }
    private void OnDrawGizmosSelected()
    {
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y< Size.y; y++)
            { 
                Gizmos.color = Color.red;
                Gizmos.DrawCube(transform.position + new Vector3(x,0,y), new Vector3(1, .1f, 1) * transform.localScale.x);
            }
        }
    }
}
