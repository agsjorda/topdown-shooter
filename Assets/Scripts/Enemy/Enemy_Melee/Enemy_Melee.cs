

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AttackData_EnemyMelee
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
public enum EnemyMeleeType { Regular, Shield, Dodge, AxeThrow }


public class Enemy_Melee : Enemy
{
    public Enemy_Visuals enemyVisuals { get; private set; }

    #region States
    public IdleState_Melee idleState { get; private set; }
    public MoveState_Melee moveState { get; private set; }
    public RecoveryState_Melee recoveryState { get; private set; }
    public ChaseState_Melee chaseState { get; private set; }
    public AttackState_Melee attackState { get; private set; }
    public DeadState_Melee deadState { get; private set; }
    public AbilityState_Melee abilityState { get; private set; }
    #endregion


    [Header("Enemy Settings")]
    public EnemyMeleeType meleeType;
    public Transform shieldTransform;
    public float dodgeCooldown;
    public float lastTimeDodge = -10;

    [Header("Axe Throw Ability")]
    public GameObject axePrefab;
    public float axeFlySpeed;
    public float axeAimTimer;
    public float axeThrowCooldown;
    public float lastTimeAxeThrown;
    public Transform axeStartPoint;


    [Header("Attack Data")]
    public AttackData_EnemyMelee attackData;
    public List<AttackData_EnemyMelee> attackList;

    protected override void Awake()
    {
        base.Awake();

        enemyVisuals = GetComponent<Enemy_Visuals>();

        idleState = new IdleState_Melee(this, stateMachine, "Idle");
        moveState = new MoveState_Melee(this, stateMachine, "Move");
        recoveryState = new RecoveryState_Melee(this, stateMachine, "Recovery");
        chaseState = new ChaseState_Melee(this, stateMachine, "Chase");
        attackState = new AttackState_Melee(this, stateMachine, "Attack");
        deadState = new DeadState_Melee(this, stateMachine, "Idle");// Idle is placeholder
        abilityState = new AbilityState_Melee(this, stateMachine, "AxeThrow");
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);
        ResetCooldown();
        InitializePerk();

        enemyVisuals.SetupLook();
        UpdateAttackData();
    }

    protected override void Update()
    {
        base.Update();

        stateMachine.currentState.Update();


    }

    override public void EnterBattleMode()
    {
        if (inBattleMode) return;

        base.EnterBattleMode();
        stateMachine.ChangeState(recoveryState);
    }

    public override void AbilityTrigger()
    {
        base.AbilityTrigger();

        moveSpeed = moveSpeed * .6f;
        EnableWeaponModel(false);
    }

    public void UpdateAttackData()
    {
        Enemy_WeaponModel currentWeapon = enemyVisuals.currentWeaponModel.GetComponent<Enemy_WeaponModel>();

        if (currentWeapon.weaponData != null) {
            attackList = new List<AttackData_EnemyMelee>(currentWeapon.weaponData.attackData);
            turnSpeed = currentWeapon.weaponData.turnSpeed;
        }
    }
    private void InitializePerk()
    {
        if (meleeType == EnemyMeleeType.AxeThrow) {
            enemyVisuals.SetupWeaponType(Enemy_MeleeWeaponType.Throw);
        }

        if (meleeType == EnemyMeleeType.Shield) {
            anim.SetFloat("ChaseIndex", 1);
            shieldTransform.gameObject.SetActive(true);
            enemyVisuals.SetupWeaponType(Enemy_MeleeWeaponType.OneHand);
        }

        if (meleeType == EnemyMeleeType.Dodge) {
            enemyVisuals.SetupWeaponType(Enemy_MeleeWeaponType.Unarmed);
        }
    }

    public override void GetHit()
    {
        base.GetHit();

        if (healthPoints <= 0)
            stateMachine.ChangeState(deadState);
        //make the impact reaction here if needed
    }

    public void EnableWeaponModel(bool active)
    {
        enemyVisuals.currentWeaponModel.gameObject.SetActive(active);
    }


    public void ActivateDodgeRoll()
    {
        if (meleeType != EnemyMeleeType.Dodge) return;

        if (stateMachine.currentState != chaseState) return;

        if (Vector3.Distance(transform.position, player.position) < 2f) return;

        float dodgeAnimationDuration = GetAnimationClipDuration("Dodge Roll");

        if (Time.time > lastTimeDodge + dodgeAnimationDuration + dodgeCooldown) {
            lastTimeDodge = Time.time;
            anim.SetTrigger("Dodge");
        }
    }

    public bool CanThrowAxe()
    {
        //Check if melee type is AxeThrow type;
        //comment line below and remove AxeThrow type to make all enemies able to throw axe
        if (meleeType != EnemyMeleeType.AxeThrow) return false;

        if (Time.time > lastTimeAxeThrown + axeThrowCooldown) {
            lastTimeAxeThrown = Time.time;
            return true;
        }
        return false;
    }

    private void ResetCooldown()
    {
        lastTimeDodge -= dodgeCooldown;
        lastTimeAxeThrown -= axeThrowCooldown;
    }
    private float GetAnimationClipDuration(string clipName)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips) {
            if (clip.name == clipName) {
                return clip.length;
            }
        }
        Debug.LogWarning(clipName + " Animation clip not found");
        return 0;
    }

    public bool IsPlayerInAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer < attackData.attackRange;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackData.attackRange);
    }
}
