using System.Collections.Generic;
using UnityEngine;
public class AttackState_Melee : EnemyState
{
    private Enemy_Melee enemy;
    private Vector3 attackDirection;
    private float attackMoveSpeed;

    private const float MAX_ATTACK_DISTANCE = 50f;
    public AttackState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Melee;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.agent.isStopped = true;
        enemy.agent.velocity = Vector3.zero;

        attackMoveSpeed = enemy.attackData.moveSpeed;
        attackDirection = enemy.transform.position + (enemy.transform.forward * MAX_ATTACK_DISTANCE);

        enemy.anim.SetFloat("AttackAnimationSpeed", enemy.attackData.annimationSpeed);
        enemy.anim.SetFloat("AttackIndex", enemy.attackData.attackIndex);
        enemy.anim.SetFloat("SlashAttackIndex", Random.Range(0, 5));
        enemy.EnableWeaponModel(true);
    }

    public override void Update()
    {
        base.Update();

        if (enemy.ManualRotationActive()) {
            enemy.FaceTarget(enemy.player.position);
            attackDirection = enemy.transform.position + (enemy.transform.forward * MAX_ATTACK_DISTANCE);
        }

        if (enemy.ManualMovementActive()) {
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, attackDirection, attackMoveSpeed * Time.deltaTime);
        }

        if (triggerCalled) {
            if (enemy.IsPlayerInAttackRange()) {
                stateMachine.ChangeState(enemy.recoveryState);
            } else
                stateMachine.ChangeState(enemy.recoveryState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        SetupNextAttack();
    }

    private void SetupNextAttack()
    {
        int recoveryIndex = PlayerClose() ? 1 : 0;

        enemy.anim.SetFloat("RecoveryIndex", recoveryIndex);
        enemy.attackData = UpdatedAttackData();
    }

    private bool PlayerClose() => Vector3.Distance(enemy.transform.position, enemy.player.position) <= 1;

    //Create Random AttackData from enemy.attackList excluding Charge attack if player is close
    private AttackData UpdatedAttackData()
    {
        List<AttackData> validAttacks = new List<AttackData>(enemy.attackList);

        if (PlayerClose()) {
            validAttacks.RemoveAll(attack => attack.attackType == AttackType_Melee.Charge);
        }

        int random = Random.Range(0, validAttacks.Count);
        return validAttacks[random];
    }
}
