using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Kaban : EntityWithHealth
{
	public float lookRadius = 10f;
	public float wanderRadius;
	public float wanderPointRadius;

	public int onBuildingEnterDamage = 15;
	public int onBuildingStayDamage = 1;

	private Building targetBuilding;
	private Vector3? wanderPoint;
	private NavMeshAgent agent;

	protected override void Start()
	{
		KabanManager.Instance.Kabans.Add(this);

		agent = GetComponent<NavMeshAgent>();
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (!IsAlive || !GameManager.Instance.hasGameStarted)
			return;

		targetBuilding = BuildingManager.Instance.Buildings.Contains(targetBuilding)
			? targetBuilding
			: BuildingManager.Instance.Buildings
				.FirstOrDefault(
					x => Vector3.Distance(x.transform.position, transform.position) < lookRadius);

		if (!(targetBuilding is null))
		{
			agent.SetDestination(targetBuilding.transform.position);
			return;
		}

		if (wanderPoint == null || agent.remainingDistance <= wanderPointRadius)
			wanderPoint = GetWanderPoint();
		if (wanderPoint.HasValue)
			agent.SetDestination(wanderPoint.Value);
	}

	private Vector3? GetWanderPoint()
	{
		Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
		if (NavMesh.SamplePosition(randomPoint, out var hit, 1f, KabanManager.Instance.WalkableAreasMask)
			&& agent.CalculatePath(hit.position, new NavMeshPath()))
			return hit.position;

		return null;
	}

	public void OnDrawGizmos()
	{
		// ReSharper disable once InvertIf
		if (wanderPoint.HasValue)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(wanderPoint.Value, wanderPointRadius);
		}
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

	public override void Destroy()
	{
		base.Destroy();

		KabanManager.Instance.Kabans.Remove(this);
	}
}