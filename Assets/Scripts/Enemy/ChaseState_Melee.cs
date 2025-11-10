using UnityEngine;

public class ChaseState_Melee : EnemyState
{
    private Enemy_Melee enemy;
    private float lastTimeUpdateDestination;

    public ChaseState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Melee;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.agent.speed = enemy.chaseSpeed;
        enemy.agent.isStopped = false;
    }

    public override void Update()
    {
        base.Update();

        enemy.transform.rotation = enemy.FaceTarget(enemy.agent.steeringTarget);

        if (enemy.IsPlayerInAttackRange()) {
            stateMachine.ChangeState(enemy.attackState);
        }


        if (CanUpdateDestination()) {
            enemy.agent.destination = enemy.player.transform.position;
        }
    }
    public override void Exit()
    {
        base.Exit();
    }

    private bool CanUpdateDestination()
    {
        if (Time.time > lastTimeUpdateDestination + .25f) {
            lastTimeUpdateDestination = Time.time;
            return true;
        }
        return false;
    }

}
