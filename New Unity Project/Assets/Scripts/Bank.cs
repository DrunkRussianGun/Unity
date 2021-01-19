using System;
using UnityEngine;
using UnityEngine.UI;

public class Bank : MonoBehaviour
{
	public static Bank Instance { get; private set; }
	
	 [HideInInspector]
    public float money;
    [HideInInspector]
    public float food;

    public float initialMoney;
    public float initialFood;
    public Text moneyView;
    public Text foodView;

    private void Awake()
    {
	    if (!(Instance is null))
		    throw new InvalidOperationException($"Объект класса {GetType()} уже существует");
	    Instance = this;
    }

    private void Start()
    {
	    money = initialMoney;
	    food = initialFood;
    }

    private void Update()
    {
        moneyView.text = money.ToString();
        foodView.text = food.ToString();
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
