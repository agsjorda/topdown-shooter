using UnityEngine;

public class MoveState_Melee : EnemyState
{
    private Enemy_Melee enemy;
    private Vector3 destination;

    public MoveState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Melee;
    }

    public override void Enter()
    {
        base.Enter();

        destination = enemy.GetPatrolDestination();

        enemy.agent.SetDestination(destination);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.IsPlayerInRange()) {
            stateMachine.ChangeState(enemy.recoveryState);
            return;
        }

        //steeringTarget is a built in NavmeshAgent method that gives the next point to move towards
        enemy.transform.rotation = enemy.FaceTarget(enemy.agent.steeringTarget);

        if (enemy.agent.remainingDistance <= enemy.agent.stoppingDistance + .5f)
            stateMachine.ChangeState(enemy.idleState);
    }

    //private Vector3 GetNextPathPoint()
    //{
    //    NavMeshAgent agent = enemy.agent;
    //    NavMeshAgent path = agent.path;

    //    if(path.corners.Length < 2)
    //        return destination;

    //    for (int i = 0; i < path.corners.length; i++) {

    //    }
    //}
}
