using System;
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
		ForceRunning,
		Stopping,
		Turning
	}

	public float lookRadius = 10f;
	public float wanderRadius;
	public float wanderPointRadius;
	
	public int onBuildingEnterDamage = 15;
	public int onBuildingStayDamage = 1;

	public float forceVelocity;
	public float forceAcceleration;

	public float hungCheckInterval;

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

	private Vector3? lastSteeringTarget;

	private Vector3 lastPosition;
	private bool isHung;
	private UpdateTimer hungChecker;
	private const float antiHungingFactor = 0.03f;

	private const float zeroVelocity = 0.1f;
	private const float zeroCos = 0.02f; // arccos 0.02 ≈ 88,854°
	private const float zeroAngleInDeg = 90 - 88.854f;

	private const float maxSlopeInDeg = 45f;
	private const float antiSlopeAngularVelocityInDeg = 540f;

	private static readonly IReadOnlyDictionary<NavigationState, Color> navigationStateColors =
		new Dictionary<NavigationState, Color>
		{
			[NavigationState.Running] = Color.clear, 
			[NavigationState.ForceRunning] = Colors.Orange,
			[NavigationState.Stopping] = Color.red,
			[NavigationState.Turning] = Color.yellow
		};

	public Kaban()
	{
		navigationActions = new Dictionary<NavigationState, Func<Vector3, NavigationState>>
		{
			[NavigationState.Running] = Run,
			[NavigationState.ForceRunning] = ForceRun,
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
		collider = GetComponent<BoxCollider>();

		lastPosition = rigidbody.position;
		hungChecker = new UpdateTimer(hungCheckInterval);

		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if (!IsAlive || !GameManager.Instance.hasGameStarted)
			return;

		if (hungChecker.Check(Time.deltaTime))
			isHung = IsHung();

		AlignWithGround();

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
		
		var position = transform.position;
		if (navigationAgent && rigidbody)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(position, position + navigationAgent.desiredVelocity);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(position, position + rigidbody.velocity);
		}

		var groundNormal = GetGroundNormal();
		if (groundNormal.HasValue && collider)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawRay(
				position,
				groundNormal.Value * (collider.size.y * transform.localScale.y * 1.25f));
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
		collision.gameObject.GetComponent<Building>()?.TakeDamage(onBuildingEnterDamage);
	}

	public void OnCollisionStay(Collision collision)
	{
		collision.gameObject.GetComponent<Building>()?.TakeDamage(onBuildingStayDamage);
	}

	private bool IsHung()
	{
		var currentPosition = rigidbody.position;
		// ReSharper disable once LocalVariableHidesMember
		var isHung = (currentPosition - lastPosition).magnitude 
			< maxVelocity * hungCheckInterval * antiHungingFactor;

		lastPosition = currentPosition;
		return isHung;
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

	private void AlignWithGround()
	{
		var groundNormal = GetGroundNormal();
		if (!groundNormal.HasValue)
			return;

		var up = transform.up;
		if (Vector3.Angle(up, groundNormal.Value) < maxSlopeInDeg)
			return;

		rigidbody.rotation *= transform.GetRotation(
			up, groundNormal.Value, antiSlopeAngularVelocityInDeg * Time.deltaTime);
	}
	
	private Vector3? GetGroundNormal()
	{
		if (!collider)
			return null;

		var scale = transform.localScale;
		var colliderCenter = transform.TransformPoint(
			Vector3.Scale(collider.center, scale));
		var colliderSize = Vector3.Scale(collider.size, scale);
		var offsets = new[]
		{
			new Vector3(-colliderSize.x, 0, -colliderSize.z),
			new Vector3(-colliderSize.x, 0, colliderSize.z),
			new Vector3(colliderSize.x, 0, -colliderSize.z),
			new Vector3(colliderSize.x, 0, colliderSize.z)
		};

		var (sum, count) = offsets
			.Select(offset => GetGroundNormal(colliderCenter + offset))
			.Where(normal => normal.HasValue)
			.Aggregate(
				(Sum: Vector3.zero, Count: 0),
				(result, normal) => (result.Sum + normal.Value, result.Count + 1));
		return count > 0 ? (Vector3?)(sum / count) : null;

		Vector3? GetGroundNormal(Vector3 from)
		{
			if (!Physics.Raycast(
				from,
				Physics.gravity,
				out var groundHit,
				GameManager.Instance.groundLayers))
				return null;

			return groundHit.normal;
		}
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
		if (isHung)
			return NavigationState.ForceRunning;

		navigationAgent.SetDestination(target);
		if (IsMissingTarget())
			return NavigationState.Stopping;
		
		UpdateVelocityAndRotation(navigationAgent.desiredVelocity, maxAcceleration, maxVelocity);
		return NavigationState.Running;
	}

	private NavigationState ForceRun(Vector3 target)
	{
		if (lastSteeringTarget.HasValue && lastSteeringTarget.Value != navigationAgent.steeringTarget)
		{
			lastSteeringTarget = null;
			return NavigationState.Running;
		}

		navigationAgent.SetDestination(target);
		if (IsMissingTarget())
		{
			lastSteeringTarget = null;
			return NavigationState.Stopping;
		}

		UpdateVelocityAndRotation(
			navigationAgent.steeringTarget - rigidbody.position, 
			forceAcceleration, 
			forceVelocity);
		lastSteeringTarget = navigationAgent.steeringTarget;
		return NavigationState.ForceRunning;
	}

	// ReSharper disable once ParameterHidesMember
	private void UpdateVelocityAndRotation(Vector3 desiredVelocity, float acceleration, float maxVelocity)
	{
		var velocityHorizontalProjection = Vector3.ProjectOnPlane(
			desiredVelocity, transform.up);
		rigidbody.rotation *= transform.GetRotation(
			transform.forward,
			velocityHorizontalProjection,
			maxAngularVelocityInDeg * Time.deltaTime);

		rigidbody.velocity += transform.GetAcceleratingVelocity(
			velocity, acceleration * Time.deltaTime, maxVelocity);
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
		var velocityHorizontalProjection = Vector3.ProjectOnPlane(
			navigationAgent.desiredVelocity, transform.up);
		if (Vector3.Angle(transform.forward, velocityHorizontalProjection) < zeroAngleInDeg)
			return NavigationState.Running;
		
		rigidbody.rotation *= transform.GetRotation(
			transform.forward,
			velocityHorizontalProjection,
			maxAngularVelocityInDeg * Time.deltaTime);

		return NavigationState.Turning;
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
}