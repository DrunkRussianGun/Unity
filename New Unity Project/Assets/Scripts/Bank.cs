using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bank : MonoBehaviour
{
    public static float money = 10;
    public static float food;
    public Text overallMoney;
    public Text overallFood;

    private void Update()
    {
        overallMoney.text = money.ToString();
        overallFood.text = food.ToString();
    }
    private void OnEnable()
    {
        Building.MoneyAdded += MoneyAdded;
        Building.FoodAdded += FoodAdded;
    }

    private void OnDisable()
    {
        Building.MoneyAdded -= MoneyAdded;
        Building.FoodAdded -= FoodAdded;
    }

    private void FoodAdded(float value)
    {
        food += value;
    }
    private void MoneyAdded(float value)
    {
        money += value;
    }
}
