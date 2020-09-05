using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Kaban : EntityWithHealth
{
    public float lookRadius = 10f;
    public int onBuildingEnterDamage = 15;
    public int onBuildingStayDamage = 1;

    private Building targetBuilding;
    private Vector3? targetPoint;
    private NavMeshAgent agent;

    protected override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (!isAlive)
            return;

        targetBuilding = BuildingManager.instance.buildings.Contains(targetBuilding)
            ? targetBuilding
            : BuildingManager.instance.buildings
                .FirstOrDefault(
                    x => Vector3.Distance(x.transform.position, transform.position) < lookRadius);

        if (targetBuilding == null)
        {
            if (targetPoint == null || agent.remainingDistance < 0.1f)
                targetPoint = GetRandomPointOnPlane();
            if (targetPoint.HasValue)
                agent.SetDestination(targetPoint.Value);
        }
        else
            agent.SetDestination(targetBuilding.transform.position);
    }

    Vector3? GetRandomPointOnPlane()
    {
        var planeForWalk = GameManager.Instance.planeForWalk;
        var planeVertices = GameManager.Instance.planeVertices;
        if (planeVertices == null)
            return null;

        var leftTop = planeForWalk.transform.TransformPoint(planeVertices[0]);
        var rightTop = planeForWalk.transform.TransformPoint(planeVertices[10]);
        var leftBottom = planeForWalk.transform.TransformPoint(planeVertices[110]);
        var rightBottom = planeForWalk.transform.TransformPoint(planeVertices[120]);
        var xAxis = rightTop - leftTop;
        var zAxis = leftBottom - leftTop;
        return leftTop + xAxis * Random.value + zAxis * Random.value;
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
