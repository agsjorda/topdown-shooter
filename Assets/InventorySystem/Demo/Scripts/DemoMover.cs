using UnityEngine;
using UnityEngine.InputSystem;

namespace InventorySystem.Demos
{
    /// WASD mover for the demo capsule so trigger-based ItemPickups can be walked over.
    [RequireComponent(typeof(Rigidbody))]
    public class DemoMover : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            Vector3 dir = Vector3.zero;
            if (keyboard.wKey.isPressed) dir.z += 1;
            if (keyboard.sKey.isPressed) dir.z -= 1;
            if (keyboard.aKey.isPressed) dir.x -= 1;
            if (keyboard.dKey.isPressed) dir.x += 1;

            if (dir != Vector3.zero) {
                rb.MovePosition(rb.position + dir.normalized * (speed * Time.deltaTime));
            }
        }
    }
}
