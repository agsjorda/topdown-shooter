

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AttackData
{
    public string attackName;
    public float attackRange;
    public float moveSpeed;
    public float attackIndex;
    [Range(1f, 2f)]
    public float annimationSpeed;
    public AttackType_Melee attackType;
}

public enum AttackType_Melee { Close, Charge }
public enum EnemyMeleeType { Regular, Shield, Dodge }


public class Enemy_Melee : Enemy
{

    public IdleState_Melee idleState { get; private set; }
    public MoveState_Melee moveState { get; private set; }
    public RecoveryState_Melee recoveryState { get; private set; }
    public ChaseState_Melee chaseState { get; private set; }
    public AttackState_Melee attackState { get; private set; }
    public DeadState_Melee deadState { get; private set; }

    [SerializeField] private Transform hiddenWeapon;
    [SerializeField] private Transform pulledWeapon;

    [Header("Enemy Settings")]
    public EnemyMeleeType meleeType;
    public Transform shieldTransform;
    public float dodgeCooldown;
    public float lastDodgeTime;

    [Header("Attack Data")]
    public AttackData attackData;
    public List<AttackData> attackList;

    protected override void Awake()
    {
        base.Awake();

        idleState = new IdleState_Melee(this, stateMachine, "Idle");
        moveState = new MoveState_Melee(this, stateMachine, "Move");
        recoveryState = new RecoveryState_Melee(this, stateMachine, "Recovery");
        chaseState = new ChaseState_Melee(this, stateMachine, "Chase");
        attackState = new AttackState_Melee(this, stateMachine, "Attack");
        deadState = new DeadState_Melee(this, stateMachine, "Idle");// Idle is placeholder

    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
        InitializeSpeciality();
    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();
    }

    private void InitializeSpeciality()
    {
        if (meleeType == EnemyMeleeType.Shield) {
            anim.SetFloat("ChaseIndex", 1);
            shieldTransform.gameObject.SetActive(true);
        }
    }

    public override void GetHit()
    {
        base.GetHit();

        if (healthPoints <= 0)
            stateMachine.ChangeState(deadState);
        //make the impact reaction here if needed
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


    public void ActivateDodgeRoll()
    {
        if (meleeType != EnemyMeleeType.Dodge) return;

        if (stateMachine.currentState != chaseState) return;

        if (Vector3.Distance(transform.position, player.position) < 2f) return;

        if (Time.time > lastDodgeTime + dodgeCooldown) {
            anim.SetTrigger("Dodge");
            lastDodgeTime = Time.time;
        }
    }
    public void PullWeapon()
    {
        // Logic to pull the weapon
        pulledWeapon.gameObject.SetActive(true);
        hiddenWeapon.gameObject.SetActive(false);
    }
}
