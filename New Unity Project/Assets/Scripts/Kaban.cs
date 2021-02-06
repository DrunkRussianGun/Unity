﻿using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
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
		Turning,
		Falling
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
	private new BoxCollider collider;

	private readonly IReadOnlyDictionary<NavigationState, Func<Vector3, NavigationState>> navigationActions;

	private Vector3 position => rigidbody.position;
	private Vector3 velocity => rigidbody.velocity;
	private float maxVelocity => navigationAgent.speed;
	private float maxAngularVelocityInDeg => navigationAgent.angularSpeed;
	private float maxAcceleration => navigationAgent.acceleration;

	private HashSet<GameObject> groundObjectsInContact = new HashSet<GameObject>();
	private bool isFalling => groundObjectsInContact.Count == 0;

	private const float zeroVelocity = 0.1f;
	private const float zeroCos = 0.02f; // arccos 0.02 ≈ 88,854°
	private const float zeroAngleInDeg = 90 - 88.854f;

	private static readonly IReadOnlyDictionary<NavigationState, Color> navigationStateColors =
		new Dictionary<NavigationState, Color>
		{
			[NavigationState.Running] = Color.clear, 
			[NavigationState.Stopping] = Color.red,
			[NavigationState.Turning] = Color.yellow,
			[NavigationState.Falling] = Color.cyan
		};

	public Kaban()
	{
		navigationActions = new Dictionary<NavigationState, Func<Vector3, NavigationState>>
		{
			[NavigationState.Running] = Run,
			[NavigationState.Stopping] = Stop,
			[NavigationState.Turning] = Turn,
			[NavigationState.Falling] = Fall
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
		collider = GetComponent<BoxCollider>();

		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (!IsAlive || !GameManager.Instance.hasGameStarted)
			return;

		// ReSharper disable once Unity.PerformanceCriticalCodeInvocation
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
		
		if (!navigationStateColors.TryGetValue(navigationState, out var navigationStateColor))
			throw new ArgumentException($"Не нашёл цвет состояния {typeof(NavigationState)}.{navigationState}");

		if (collider)
		{
			var colliderSize = Vector3.Scale(collider.size, transform.localScale);
			var longestColliderEdge = Mathf.Max(colliderSize.x, colliderSize.y, colliderSize.z);
			Gizmos.color = navigationStateColor;
			Gizmos.DrawWireCube(
				transform.TransformPoint(collider.center),
				new Vector3(longestColliderEdge, longestColliderEdge, longestColliderEdge));
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

	public void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer.In(GameManager.Instance.groundLayers))
		{
			groundObjectsInContact.Add(collision.gameObject);
			RemoveNonExistentGroundObjects();
		}

		collision.gameObject.GetComponent<Building>()?.TakeDamage(onBuildingEnterDamage);
	}

	public void OnCollisionStay(Collision collision)
	{
		collision.gameObject.GetComponent<Building>()?.TakeDamage(onBuildingStayDamage);
	}

	public void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.layer.In(GameManager.Instance.groundLayers))
		{
			groundObjectsInContact.Remove(collision.gameObject);
			RemoveNonExistentGroundObjects();
		}
	}

	private Building GetTargetBuilding()
	{
		var buildings = BuildingManager.Instance.Buildings;
		if (buildings.Contains(targetBuilding))
			return targetBuilding;

		var buildingsWithinLookRadius = buildings
			.Where(building => Vector3.Distance(building.transform.position, position) < lookRadius)
			.ToArray();
		if (buildingsWithinLookRadius.Length == 0)
			return null;

		var tower = buildingsWithinLookRadius
			.FirstOrDefault(building => building.GetComponent<Tower>());
		if (tower)
			return tower;

		return buildingsWithinLookRadius.FirstOrDefault();
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
		if (isFalling)
			return NavigationState.Falling;

		navigationAgent.SetDestination(target);
		if (IsMissingTarget())
			return NavigationState.Stopping;

		var velocityHorizontalProjection = Vector3.ProjectOnPlane(
			navigationAgent.desiredVelocity, transform.up);
		rigidbody.rotation *= transform.GetTurn(
			velocityHorizontalProjection,
			maxAngularVelocityInDeg * Time.deltaTime);
		var velocityVerticalProjection = Vector3.ProjectOnPlane(
			navigationAgent.desiredVelocity, transform.right);
		rigidbody.rotation *= transform.GetTurn(
			velocityVerticalProjection,
			maxAngularVelocityInDeg * Time.deltaTime);

		rigidbody.velocity += transform.GetAcceleratingVelocity(
			velocity, maxAcceleration * Time.deltaTime, maxVelocity);

		return NavigationState.Running;
	}

	private bool IsMissingTarget()
	{
		if (velocity.magnitude < zeroVelocity)
			return false;

		var up = transform.up;
		var desiredVelocityProjection = Vector3.ProjectOnPlane(
			navigationAgent.desiredVelocity, up);
		var velocityNormal = Vector3
			.ProjectOnPlane(velocity, up)
			.GetNormalInPlaneWith(desiredVelocityProjection);

		var angleInDeg = Vector3.Angle(desiredVelocityProjection, velocityNormal);
		var cos = Mathf.Cos(angleInDeg * Mathf.Deg2Rad);
		if (cos < 0)
			return true;
		if (cos < zeroCos)
			return false;

		var minRadius = velocity.magnitude / (maxAngularVelocityInDeg * Mathf.Deg2Rad);
		var radiusToReachTarget = Vector3.Distance(navigationAgent.steeringTarget, position)
			/ (cos * 2);
		return radiusToReachTarget < minRadius;
	}

	private NavigationState Stop(Vector3 target)
	{
		if (isFalling)
			return NavigationState.Falling;
		if (velocity.magnitude < zeroVelocity)
			return NavigationState.Turning;

		var deceleratingVelocity = -transform.GetAcceleratingVelocity(
			velocity, maxAcceleration * Time.deltaTime, maxVelocity);
		var deceleratedVelocity = velocity - deceleratingVelocity;
		rigidbody.velocity = deceleratedVelocity.magnitude < velocity.magnitude
			? deceleratedVelocity
			: Vector3.zero;

		return NavigationState.Stopping;
	}

	private NavigationState Turn(Vector3 target)
	{
		if (isFalling)
			return NavigationState.Falling;

		var velocityHorizontalProjection = Vector3.ProjectOnPlane(
			navigationAgent.desiredVelocity, transform.up);
		if (Vector3.Angle(transform.forward, velocityHorizontalProjection) < zeroAngleInDeg)
			return NavigationState.Running;
		
		rigidbody.rotation *= transform.GetTurn(
			velocityHorizontalProjection,
			maxAngularVelocityInDeg * Time.deltaTime);

		return NavigationState.Turning;
	}

	private NavigationState Fall(Vector3 target)
		=> isFalling ? NavigationState.Falling : NavigationState.Running;

	private void KeepNavigationAgentAtRigidbody()
	{
		var transformScale = transform.localScale;
		var radiusScale = Mathf.Min(transformScale.x, transformScale.y, transformScale.z);
		if (Vector3.Distance(navigationAgent.nextPosition, rigidbody.position)
		    > navigationAgent.radius * radiusScale / 2)
			navigationAgent.nextPosition = position;
		navigationAgent.velocity = velocity;
	}

	private void RemoveNonExistentGroundObjects()
		=> groundObjectsInContact.RemoveWhere(x => !x);
}