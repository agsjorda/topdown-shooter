

using UnityEngine;

[System.Serializable]
public struct AttackData
{
    public float attackRange;
    public float moveSpeed;
    public float attackIndex;
    [Range(1f, 2f)]
    public float annimationSpeed;
}
public class Enemy_Melee : Enemy
{

    public IdleState_Melee idleState { get; private set; }
    public MoveState_Melee moveState { get; private set; }
    public RecoveryState_Melee recoveryState { get; private set; }
    public ChaseState_Melee chaseState { get; private set; }
    public AttackState_Melee attackState { get; private set; }

    //[SerializeField] private Transform hiddenWeapon;
    //[SerializeField] private Transform pulledWeapon;

    [Header("Attack Data")]
    public AttackData attackData;

    protected override void Awake()
    {
        base.Awake();

        idleState = new IdleState_Melee(this, stateMachine, "Idle");
        moveState = new MoveState_Melee(this, stateMachine, "Move");
        recoveryState = new RecoveryState_Melee(this, stateMachine, "Recovery");
        chaseState = new ChaseState_Melee(this, stateMachine, "Chase");
        attackState = new AttackState_Melee(this, stateMachine, "Attack");

    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);
    }

    public bool IsPlayerInAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer < attackData.attackRange;
    }

    //public void PullWeapon()
    //{
    //    // Logic to pull the weapon
    //    pulledWeapon.gameObject.SetActive(true);
    //    hiddenWeapon.gameObject.SetActive(false);
    //}
}
