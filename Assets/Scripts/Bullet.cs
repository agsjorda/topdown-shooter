using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private GameObject bulletImpactFX;
    private Rigidbody rb => GetComponent<Rigidbody>();
    private void OnCollisionEnter(Collision collision) {
        //rb.constraints = RigidbodyConstraints.FreezeAll;
        CreateImpactFX(collision);

        // Instead of destroying the bullet, we can return it to an object pool
        ObjectPool.instance.ReturnBulletToPool(gameObject);
    }

    private void CreateImpactFX(Collision collision) {
        if (collision.contacts.Length > 0) {
            ContactPoint contact = collision.contacts[0];

            GameObject newImpactFX = Instantiate(bulletImpactFX, contact.point, Quaternion.LookRotation(contact.normal));

            Destroy(newImpactFX, 1f);
        }
    }
}
