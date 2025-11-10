using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    private Enemy_Melee enemy;
    [SerializeField] private int durability;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy_Melee>();
    }
    public void ReduceDurability()
    {
        durability--;

        if (durability <= 0) {
            Destroy(gameObject);
            enemy.anim.SetFloat("ChaseIndex", 0); // Enable default chase animations
        }
    }
}
