using System.Linq;
using Helpers;
using UnityEngine;

public class Tower : MonoBehaviour
{
    private Building building;
    
    public float shootInterval;
    public float shootDistance;
    public int damage;
    private UpdateTimer damageTimer;
    private Kaban shootedKaban;
    private LineRenderer shootRayRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        building = GetComponent<Building>();
        damageTimer = new UpdateTimer(shootInterval);
        shootRayRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!building.IsActivated)
            return;

        if (damageTimer.Check(Time.deltaTime))
        {
            Kaban minHealthKaban = null;
            var closeEnoughKabans = GameObject.FindGameObjectsWithTag("Kaban")
                .Select(x => x.GetComponent<Kaban>())
                .Where(CanBeShooted);
            foreach (var kaban in closeEnoughKabans)
                if (minHealthKaban == null || kaban.currentHealth < minHealthKaban.currentHealth)
                    minHealthKaban = kaban;
            shootedKaban = minHealthKaban;

            if (shootedKaban != null)
                shootedKaban.TakeDamage(damage);
        }

        if (shootedKaban != null && shootedKaban.gameObject)
        {
            shootRayRenderer.SetPosition(0, transform.position);
            shootRayRenderer.SetPosition(1, shootedKaban.transform.position);
            shootRayRenderer.enabled = true;
        }
        else
            shootRayRenderer.enabled = false;
    }

    private bool CanBeShooted(Kaban kaban)
        => Vector3.Distance(kaban.transform.position, transform.position) < shootDistance;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootDistance);
    }
}
