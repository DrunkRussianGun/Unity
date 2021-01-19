using System;
using UnityEngine;

public abstract class EntityWithHealth : MonoBehaviour
{
    protected bool IsAlive = true;
    public int maxHealth;
    public int currentHealth;
    public float healthRestoringDelay;
    public float healthInterval;
    public int healthIncrement;
    protected UpdateTimer HealthDelay;
    protected UpdateTimer HealthTimer;
    protected HealthBar HealthBar;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        HealthDelay = new UpdateTimer(healthRestoringDelay, 1, true);
        HealthTimer = new UpdateTimer(healthInterval);

        HealthBar = GetComponentInChildren<HealthBar>();
        HealthBar?.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (currentHealth <= 0)
            Destroy();
        if (HealthDelay.Check(Time.deltaTime) && HealthTimer.Check(Time.deltaTime))
            Heal(healthIncrement);
    }

    public virtual void Heal(int heal)
    {
        currentHealth = Math.Min(maxHealth, currentHealth + heal);
        HealthBar?.SetHealth(currentHealth);
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        HealthDelay.ResetAll();
        HealthTimer.ResetTimer();
        HealthBar?.SetHealth(currentHealth);
    }

    public virtual void Destroy()
    {
        IsAlive = false;
        Destroy(gameObject);
    }
}
