using System;
using UnityEngine;

public abstract class EntityWithHealth : MonoBehaviour
{
    protected bool isAlive = true;
    public int maxHealth;
    public int currentHealth;
    public float healthRestoringDelay;
    public float healthInterval;
    public int healthIncrement;
    protected UpdateTimer healthDelay;
    protected UpdateTimer healthTimer;
    protected HealthBar healthBar;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        healthDelay = new UpdateTimer(healthRestoringDelay, 1, true);
        healthTimer = new UpdateTimer(healthInterval);

        healthBar = GetComponentInChildren<HealthBar>();
        healthBar?.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (currentHealth <= 0)
            Destroy();
        if (healthDelay.Check(Time.deltaTime) && healthTimer.Check(Time.deltaTime))
            Heal(healthIncrement);
    }

    public virtual void Heal(int heal)
    {
        currentHealth = Math.Min(maxHealth, currentHealth + heal);
        healthBar?.SetHealth(currentHealth);
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthDelay.ResetAll();
        healthTimer.ResetTimer();
        healthBar?.SetHealth(currentHealth);
    }

    public virtual void Destroy()
    {
        isAlive = false;
        Destroy(gameObject);
    }
}
