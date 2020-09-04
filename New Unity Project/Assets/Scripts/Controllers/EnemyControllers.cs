using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyControllers : MonoBehaviour
{
    public float lookRadius = 0.01f;

    Transform target;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        target = BuildingManager.instance.buildings.LastOrDefault()?.transform;
        if (target == null)
            return;
        
        if (agent.destination != target.position)
            agent.destination = target.position;
    }

    void OnDrawGizmosSelected()
    {
    	Gizmos.color = Color.red;
    	Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
