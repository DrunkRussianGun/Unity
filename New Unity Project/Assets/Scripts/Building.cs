using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	private bool isActivated;

    public Vector2Int Size = Vector2Int.one;
    public int maxhp = 1000;
    private int currenthp;
    public HealthBar healthBar;

    public static event Action<float> MoneyAdded;
    public static event Action<float> FoodAdded;
    public bool isFood;
    public GameObject canBeUpgradedTo;

    public float moneyAndFoodInterval;
    public float moneyIncrement;
    public float foodIncrement;
    private UpdateTimer moneyAndFoodTimer;

	public float healthRestoringDelay;
	public float healthInterval;
	public int healthIncrement;
	private UpdateTimer healthDelay;
	private UpdateTimer healthTimer;

    private void Start()
    {
		moneyAndFoodTimer = new UpdateTimer(moneyAndFoodInterval);
		healthDelay = new UpdateTimer(healthRestoringDelay, 1, true);
		healthTimer = new UpdateTimer(healthInterval);
    }

    private void Update()
    {
		if (!isActivated)
			return;

        if (currenthp <= 0)
			Destroy();
        if (moneyAndFoodTimer.Check(Time.deltaTime))
        {
            MoneyAdded.Invoke(moneyIncrement);
            if (isFood)
				FoodAdded.Invoke(foodIncrement);
        }
		if (healthDelay.Check(Time.deltaTime) && healthTimer.Check(Time.deltaTime))
			Heal(healthIncrement);
    }

	public void Activate()
	{
        BuildingManager.instance.buildings.Add(this);
		gameObject.GetComponent<BoxCollider>().enabled = true;

		currenthp = maxhp;
        healthBar.SetMaxHealth(maxhp);

		isActivated = true;
	}

	public void Heal(int heal)
	{
		currenthp = Math.Min(maxhp, currenthp + heal);
        healthBar.SetHealth(currenthp);
	}

    public void TakeDamage(int damage)
    {
        currenthp -= damage;
		healthDelay.ResetAll();
		healthTimer.ResetTimer();
        healthBar.SetHealth(currenthp);
    }

	public void Destroy()
	{
        BuildingManager.instance.buildings.Remove(this);
		isActivated = false;
		Destroy(gameObject);
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

    void OnCollisionEnter(Collision collision)
    {
		PushAway(collision.rigidbody, BuildingManager.instance.pushingAwayForceOnEnter, ForceMode.Impulse);
    }

    void OnCollisionStay(Collision collision)
    {
		PushAway(collision.rigidbody, BuildingManager.instance.pushingAwayForceOnStay, ForceMode.VelocityChange);
    }

	void PushAway(Rigidbody rigidbody, float forceMultiplier, ForceMode forceMode)
	{
		if (rigidbody == null)
			return;

		var normalizedVector = rigidbody.transform.position - transform.position;
		normalizedVector.Normalize();
		rigidbody.AddForce(normalizedVector * forceMultiplier, forceMode);
	}
}
