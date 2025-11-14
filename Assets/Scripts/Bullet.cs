using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float impactForce;

    private BoxCollider cd;
    private Rigidbody rb;
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;

    [SerializeField] private GameObject bulletImpactFX;

    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletDisabled;


    private void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public void BulletSetup(float flyDistance, float impactForce)
    {
        this.impactForce = impactForce;

        bulletDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;
        trailRenderer.time = 0.25f;
        startPosition = transform.position;
        this.flyDistance = flyDistance + .5f; //.5f is a length of the tip of the aim laser (check method UpdateAimVisuals in PlayerAim.cs)
    }

    private void Update()
    {
        FadeTrailIfNeeded();
        DisableBulletIfNeeded();
        ReturnToPoolIfNeeded();

    }

    private void ReturnToPoolIfNeeded()
    {
        if (trailRenderer.time < 0)
            ReturnBulletToPool();
    }

    private void ReturnBulletToPool() => ObjectPool.instance.ReturnObject(gameObject);

    private void DisableBulletIfNeeded()
    {
        if (Vector3.Distance(startPosition, transform.position) >= flyDistance && !bulletDisabled) {

            cd.enabled = false;
            meshRenderer.enabled = false;
            bulletDisabled = true;
        }
    }

    private void FadeTrailIfNeeded()
    {
        if (Vector3.Distance(startPosition, transform.position) >= flyDistance - 1.5f)
            trailRenderer.time -= 2 * Time.deltaTime; //2 is chosen to make the trail disappear faster
    }

    private void OnCollisionEnter(Collision collision)
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

    private void CreateImpactFX(Collision collision)
    {
        if (collision.contacts.Length > 0) {
            ContactPoint contact = collision.contacts[0];

            GameObject newImpactFX = ObjectPool.instance.GetObject(bulletImpactFX);
            newImpactFX.transform.position = contact.point;

            ObjectPool.instance.ReturnObject(newImpactFX, 1f);
        }
    }
}
