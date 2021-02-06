using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class Kaban : EntityWithHealth
{
	private enum NavigationState
	{
		Running,
		Stopping,
		Turning
	}

	public float lookRadius = 10f;
	public float wanderRadius;
	public float wanderPointRadius; 
	
	public int onBuildingEnterDamage = 15;
	public int onBuildingStayDamage = 1;

	private Building targetBuilding;
	private Vector3? wanderPoint;

	private NavigationState navigationState;
	private NavMeshAgent navigationAgent;
	private new Rigidbody rigidbody;

	private readonly IReadOnlyDictionary<NavigationState, Func<Vector3, NavigationState>> navigationActions;

	private Vector3 position => rigidbody.position;
	private Vector3 velocity => rigidbody.velocity;
	private float maxVelocity => navigationAgent.speed;
	private float maxAngularVelocityInDeg => navigationAgent.angularSpeed;
	private float maxAcceleration => navigationAgent.acceleration;

	public Kaban()
	{
		navigationActions = new Dictionary<NavigationState, Func<Vector3, NavigationState>>
		{
			[NavigationState.Running] = Run,
			[NavigationState.Stopping] = Stop,
			[NavigationState.Turning] = Turn
		};
	}

	protected override void Start()
	{
		KabanManager.Instance.Kabans.Add(this);

		navigationAgent = GetComponent<NavMeshAgent>();
		navigationAgent.updatePosition = false;
		navigationAgent.updateUpAxis = false;
		navigationAgent.updateRotation = false;

		rigidbody = GetComponent<Rigidbody>();

		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (!IsAlive || !GameManager.Instance.hasGameStarted)
			return;

		targetBuilding = GetTargetBuilding();
		if (targetBuilding)
		{
			NavigateTo(targetBuilding.transform.position);
			return;
		}
		
		if (wanderPoint == null
		    || navigationAgent.destination == wanderPoint
				&& navigationAgent.remainingDistance <= wanderPointRadius)
			wanderPoint = GetWanderPoint();
		if (wanderPoint.HasValue)
			NavigateTo(wanderPoint.Value);
	}

	public override void Destroy()
	{
		base.Destroy();

		KabanManager.Instance.Kabans.Remove(this);
	}

	public void OnDrawGizmos()
	{
		// ReSharper disable once InvertIf
		if (wanderPoint.HasValue)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(wanderPoint.Value, wanderPointRadius);
		}
		
		if (navigationAgent && rigidbody)
		{
			var position = transform.position;
			
			Gizmos.color = Color.red;
			Gizmos.DrawLine(position, position + navigationAgent.desiredVelocity);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(position, position + rigidbody.velocity);
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, lookRadius);

		if (navigationAgent)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(navigationAgent.steeringTarget, 0.2f);
		}
	}

	private Building GetTargetBuilding()
	{
		var buildings = BuildingManager.Instance.Buildings;
		return buildings.Contains(targetBuilding)
			? targetBuilding
			: buildings.FirstOrDefault(
				building => Vector3.Distance(building.transform.position, position) < lookRadius);
	}

	private Vector3? GetWanderPoint()
	{
		Vector3 randomPoint = position + Random.insideUnitSphere * wanderRadius;
		if (NavMesh.SamplePosition(randomPoint, out var hit, 1f, KabanManager.Instance.WalkableAreasMask)
			&& navigationAgent.CalculatePath(hit.position, new NavMeshPath()))
			return hit.position;

		return null;
	}

	private void NavigateTo(Vector3 target)
	{
		if (!navigationActions.TryGetValue(navigationState, out var navigationAction))
			throw new ArgumentOutOfRangeException(nameof(navigationState), navigationState.ToString());
		navigationState = navigationAction.Invoke(target);

		KeepNavigationAgentAtRigidbody();
	}

	private NavigationState Run(Vector3 target)
	{
		navigationAgent.SetDestination(target);
		if (IsMissingTarget())
			return NavigationState.Stopping;

		var velocityHorizontalProjection = Vector3.ProjectOnPlane(
			navigationAgent.desiredVelocity, transform.up);
		rigidbody.rotation *= transform.GetTurn(
			velocityHorizontalProjection,
			maxAngularVelocityInDeg * Time.deltaTime);

		rigidbody.velocity += transform.GetAcceleratingVelocity(
			velocity, maxAcceleration * Time.deltaTime, maxVelocity);

		return NavigationState.Running;
	}

	private bool IsMissingTarget() => false;

	private NavigationState Stop(Vector3 target)
	{
		throw new NotImplementedException();
	}

	private NavigationState Turn(Vector3 target)
	{
		throw new NotImplementedException();
	}

	private void KeepNavigationAgentAtRigidbody()
	{
		var transformScale = transform.localScale;
		var radiusScale = Mathf.Min(transformScale.x, transformScale.y, transformScale.z);
		if (Vector3.Distance(navigationAgent.nextPosition, rigidbody.position)
		    > navigationAgent.radius * radiusScale / 2)
			navigationAgent.nextPosition = position;
		navigationAgent.velocity = velocity;
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