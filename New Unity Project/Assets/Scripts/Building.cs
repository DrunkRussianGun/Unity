using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Vector2Int Size = Vector2Int.one;
    public int maxhp = 1000;
    public int currenthp;
    public HealthBar healthBar;

    public static event Action<float> MoneyAdded;
    public static event Action<float> FoodAdded;
    public bool isFood;
    public GameObject canBeUpgradedTo;
    [SerializeField] private float moneyTimer;
    [SerializeField] private float moneyValue;
    [SerializeField] private float foodValue;

    private float timer;

    private void Start()
    {
        currenthp = maxhp;
        healthBar.SetMaxHealth(maxhp);
    }
    private void Update()
    {
        if (currenthp <= 0)
			Destroy();
        if (timer >= moneyTimer)
        {
            timer = 0;
            MoneyAdded.Invoke(moneyValue);
            if (isFood) FoodAdded.Invoke(foodValue);
        }

        timer += Time.deltaTime;
    }

	public void Activate()
	{
        BuildingManager.instance.buildings.Add(this);
		gameObject.GetComponent<BoxCollider>().enabled = true;
	}

    public void TakeDamage(int damage)
    {
        currenthp -= damage;
        healthBar.SetHealth(currenthp);
    }

	public void Destroy()
	{
        BuildingManager.instance.buildings.Remove(this);
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
