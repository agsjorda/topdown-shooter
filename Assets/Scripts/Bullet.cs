using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float impactForce;

    private BoxCollider cd;
    private Rigidbody rb;
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;

    [SerializeField] private GameObject bulletImpactFX;

    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletDisabled;


    protected virtual void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public void BulletSetup(float flyDistance = 100, float impactForce = 100)
    {
        this.impactForce = impactForce;

        bulletDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;
        trailRenderer.time = 0.25f;
        startPosition = transform.position;
        this.flyDistance = flyDistance + .5f; //.5f is a length of the tip of the aim laser (check method UpdateAimVisuals in PlayerAim.cs)
    }

    protected virtual void Update()
    {
        FadeTrailIfNeeded();
        DisableBulletIfNeeded();
        ReturnToPoolIfNeeded();

    }

    protected virtual void ReturnToPoolIfNeeded()
    {
        if (trailRenderer.time < 0)
            ReturnBulletToPool();
    }

    protected void ReturnBulletToPool()
    {
        if (ObjectPool.instance != null)
            ObjectPool.instance.ReturnObject(gameObject);
    }

    protected void DisableBulletIfNeeded()
    {
        if (Vector3.Distance(startPosition, transform.position) >= flyDistance && !bulletDisabled) {

            cd.enabled = false;
            meshRenderer.enabled = false;
            bulletDisabled = true;
        }
    }

    protected void FadeTrailIfNeeded()
    {
        if (Vector3.Distance(startPosition, transform.position) >= flyDistance - 1.5f)
            trailRenderer.time -= 2 * Time.deltaTime; //2 is chosen to make the trail disappear faster
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        CreateImpactFX(collision);
        // Instead of destroying the bullet, we can return it to an object pool
        ReturnBulletToPool();

        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
        Enemy_Shield enemyShield = collision.gameObject.GetComponent<Enemy_Shield>();

        if (enemyShield != null) {
            enemyShield.ReduceDurability();
            return;
        }

        if (enemy != null) {
            Vector3 forceDirection = rb.linearVelocity.normalized * impactForce;
            Rigidbody hitRigidbody = collision.collider.attachedRigidbody;
            enemy.GetHit();
            enemy.DeathImpact(forceDirection, collision.contacts[0].point, hitRigidbody);
        }

    }

    protected void CreateImpactFX(Collision collision)
    {
        if (collision.contacts.Length > 0) {
            ContactPoint contact = collision.contacts[0];

            GameObject newImpactFX = ObjectPool.instance.GetObject(bulletImpactFX);
            newImpactFX.transform.position = contact.point;

            ObjectPool.instance.ReturnObject(newImpactFX, 1f);
        }
    }
}
