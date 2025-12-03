using UnityEngine;

public class Enemy_Axe : MonoBehaviour
{
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform axeVisual;

    private Vector3 direction;
    private Transform player;
    private float flySpeed;
    private float rotationSpeed = 1500f;
    private float timer = 1;
    private float lifetime = 5f;
    private float maxDistance = 50f;
    private Vector3 startPosition;

    public void AxeSetup(float flySpeed, Transform player, float timer)
    {
        this.flySpeed = flySpeed;
        this.player = player;
        this.timer = timer;
        this.lifetime = 5f;
        this.startPosition = transform.position;
    }

    private void Update()
    {
        axeVisual.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

        timer -= Time.deltaTime;
        lifetime -= Time.deltaTime;

        // Return to pool if lifetime expires OR too far away
        if (lifetime <= 0f || Vector3.Distance(startPosition, transform.position) >= maxDistance) {
            ReturnAxeToPool();
            return;
        }

        if (timer > 0)
            direction = player.position + Vector3.up - transform.position;

        rb.linearVelocity = direction.normalized * flySpeed;
        transform.forward = rb.linearVelocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Don't collide with enemy that threw the axe - check by component instead of tag
        if (other.GetComponent<Enemy>() != null) return;

        Bullet bullet = other.GetComponent<Bullet>();
        Player player = other.GetComponent<Player>();

        if (bullet != null || player != null) {
            GameObject newImpactFX = ObjectPool.instance.GetObject(impactEffect);
            newImpactFX.transform.position = other.ClosestPoint(transform.position);

            ReturnAxeToPool();
            ObjectPool.instance.ReturnObject(newImpactFX, 1f);
        }
    }

    private void ReturnAxeToPool()
    {
        if (ObjectPool.instance != null)
            ObjectPool.instance.ReturnObject(gameObject);
    }

    // Optional: Reset when enabled from pool
    private void OnEnable()
    {
        // Reset any necessary state here
        if (rb != null) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}