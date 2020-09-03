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
        target = BuildingManager.instance.buildings.FirstOrDefault()?.transform;
        if (target == null)
        {
            return;
        }
        
        float distance = Vector3.Distance(target.position, transform.position);
        
        if (distance <= lookRadius)
        {
            agent.SetDestination(target.position);
        
            /*if (distance <= agent.stoppingDistance)
            {
                //Attack the target
            }*/
        }
    }

    /*void Facetarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }*/

    void OnDrawGizmosSelected  ()
    {
    	Gizmos.color = Color.red;
    	Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
