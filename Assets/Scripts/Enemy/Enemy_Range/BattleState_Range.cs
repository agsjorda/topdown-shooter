using UnityEngine;

public class BattleState_Range : EnemyState
{
    private Enemy_Range enemy;

    private float lastTimeShot = -10;
    private int bulletsShot = 0;
    public BattleState_Range(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName) : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = enemyBase as Enemy_Range;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        enemy.FaceTarget(enemy.player.position);

        if (IsWeaponOutOfBullets()) {

            if (IsWeaponOnCooldown())
                AttemptToResetWeapon();

            return;
        }

        if (CanShoot()) {
            Shoot();
        }
    }

    private void AttemptToResetWeapon() => bulletsShot = 0;
    private bool IsWeaponOnCooldown() => Time.time > lastTimeShot + enemy.weaponCooldownTime;
    private bool IsWeaponOutOfBullets() => bulletsShot >= enemy.bulletsToShoot;
    private bool CanShoot() => Time.time > lastTimeShot + 1 / enemy.fireRate;
    private void Shoot()
    {
        enemy.FireSingleBullet();
        lastTimeShot = Time.time;
        bulletsShot++;
    }
}
