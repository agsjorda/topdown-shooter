using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected int healthPoints = 25;

    [Header("Idle data")]
    public float idleTime;
    public float detectionRange;

    [Header("Move data")]
    public float moveSpeed;
    public float chaseSpeed;
    public float turnSpeed;
    private bool manualMovement;
    private bool manualRotation;

    [SerializeField] private Transform[] patrolPoints;
    private Vector3[] patrolPointsPosition;
    private int currentPatrolIndex;

    public bool inBattleMode;


    public Transform player { get; private set; }
    public Animator anim { get; private set; }
    public NavMeshAgent agent { get; private set; }

    public EnemyStateMachine stateMachine { get; private set; }

    protected virtual void Awake()
    {
        stateMachine = new EnemyStateMachine();

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        player = GameObject.Find("Player").GetComponent<Transform>();
    }

    protected virtual void Start()
    {
        InitializePatrolPoints();
    }


    protected virtual void Update()
    {
    }

    protected bool ShouldEnterBattleMode()
    {
        bool playerInRange = Vector3.Distance(transform.position, player.position) <= detectionRange;

        if (playerInRange && !inBattleMode) {
            EnterBattleMode();
            return true;
        }
        return false;
    }

    public virtual void EnterBattleMode()
    {
        inBattleMode = true;
    }

    public virtual void GetHit()
    {
        EnterBattleMode();
        healthPoints--;
    }

    public virtual void DeathImpact(Vector3 force, Vector3 impactPoint, Rigidbody rb)
    {
        StartCoroutine(DeathImpactCouroutine(force, impactPoint, rb));
    }

    private IEnumerator DeathImpactCouroutine(Vector3 force, Vector3 impactPoint, Rigidbody rb)
    {
        yield return new WaitForSeconds(.1f);

        rb.AddForceAtPosition(force, impactPoint, ForceMode.Impulse);
    }
    public void FaceTarget(Vector3 target)
    {
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);

        Vector3 currentEulerAngles = transform.rotation.eulerAngles;

        float yRotation = Mathf.LerpAngle(currentEulerAngles.y, targetRotation.eulerAngles.y, turnSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(currentEulerAngles.x, yRotation, currentEulerAngles.z);
    }

    #region Animation Events
    public void ActivateManualMovement(bool activate) => this.manualMovement = activate;
    public bool ManualMovementActive() => manualMovement;
    public void ActivateManualRotation(bool activate) => this.manualRotation = activate;
    public bool ManualRotationActive() => manualRotation;

    public void AnimationTrigger() => stateMachine.currentState.AnimationTrigger();
    public virtual void AbilityTrigger() => stateMachine.currentState.AbilityTrigger();
    #endregion

    #region Patrol Logic
    public Vector3 GetPatrolDestination()
    {

        Vector3 destination = patrolPointsPosition[currentPatrolIndex];

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
            currentPatrolIndex = 0;

        return destination;
    }

    private void InitializePatrolPoints()
    {
        patrolPointsPosition = new Vector3[patrolPoints.Length];
        for (int i = 0; i < patrolPoints.Length; i++) {
            patrolPointsPosition[i] = patrolPoints[i].position;
            patrolPoints[i].gameObject.SetActive(false);
        }
    }
    #endregion
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

}
