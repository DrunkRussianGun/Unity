using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyControllers : MonoBehaviour
{
    public float lookRadius = 10f;
    public int onBuildingEnterDamage = 15;
    public int onBuildingStayDamage = 1;

    Building targetBuilding;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        targetBuilding = BuildingManager.instance.buildings.Contains(targetBuilding)
            ? targetBuilding
            : BuildingManager.instance.buildings
                .FirstOrDefault(
                    x => Vector3.Distance(x.transform.position, transform.position) < lookRadius);
        
        if (targetBuilding == null)
            agent.ResetPath();
        else
            agent.SetDestination(targetBuilding.transform.position);
    }

    void OnDrawGizmosSelected()
    {
    	Gizmos.color = Color.red;
    	Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.GetComponent<Building>()?.TakeDamage(onBuildingEnterDamage);
    }
    
    void OnCollisionStay(Collision collision)
    {
        collision.gameObject.GetComponent<Building>()?.TakeDamage(onBuildingStayDamage);
    }
}
