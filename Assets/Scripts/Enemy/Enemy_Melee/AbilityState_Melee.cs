using UnityEngine;

public class AbilityState_Melee : EnemyState
{
    Enemy_Melee enemy;
    private Vector3 movementDirection;
    private const float MAX_MOVEMENT_DISTANCE = 20f;

    private float moveSpeed;

    public AbilityState_Melee(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Melee;
    }

    public override void Enter()
    {
        base.Enter();
        enemy.EnableWeaponModel(true);

        moveSpeed = enemy.moveSpeed;

        movementDirection = enemy.transform.position + (enemy.transform.forward * MAX_MOVEMENT_DISTANCE);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.moveSpeed = moveSpeed;
        enemy.anim.SetFloat("RecoveryIndex", 0);
    }

    public override void Update()
    {
        base.Update();
        if (enemy.ManualRotationActive()) {
            enemy.FaceTarget(enemy.player.position);
            movementDirection = enemy.transform.position + (enemy.transform.forward * MAX_MOVEMENT_DISTANCE);
        }

        if (enemy.ManualMovementActive()) {
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, movementDirection, enemy.moveSpeed * Time.deltaTime);
        }

        if (triggerCalled) {
            stateMachine.ChangeState(enemy.recoveryState);
        }
    }
    public override void AbilityTrigger()
    {
        base.AbilityTrigger();

        // Safely spawn axe from pool at axeStartPoint
        GameObject newAxe = ObjectPool.instance.SpawnFromPool(
            enemy.axePrefab,
            enemy.axeStartPoint,
            Vector3.zero,
            Quaternion.identity,
            keepParented: false
        );

        // Ensure fresh physics state
        var rb = newAxe.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Initialize the axe
        newAxe.GetComponent<Enemy_Axe>().AxeSetup(
            enemy.axeFlySpeed,
            enemy.player,
            enemy.axeAimTimer
        );
    }


    //public override void AbilityTrigger()
    //{
    //    base.AbilityTrigger();

    //    GameObject newAxe = ObjectPool.instance.GetObject(enemy.axePrefab);

    //    newAxe.transform.position = enemy.axeStartPoint.position;
    //    newAxe.GetComponent<Enemy_Axe>().AxeSetup(enemy.axeFlySpeed, enemy.player, enemy.axeAimTimer);
    //}
}
