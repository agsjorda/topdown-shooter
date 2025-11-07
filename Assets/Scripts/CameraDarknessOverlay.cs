using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class CameraDarknessOverlay : MonoBehaviour
{
    [Range(0f, 1f)] public float darkness = 0.5f; // Transparency control
    private SpriteRenderer sr;
    private Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = GetComponentInParent<Camera>();
    }

    void LateUpdate()
    {
        if (sr == null || cam == null) return;

        // Keep it always in front of the camera
        transform.localPosition = new Vector3(0, 0, cam.nearClipPlane + 0.01f);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        // Adjust overlay size to fill the view
        float h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * cam.nearClipPlane * 2f;
        float w = h * cam.aspect;
        transform.localScale = new Vector3(w, h, 1f) * 50f; // multiplier makes sure it covers screen

        // Apply darkness color
        sr.color = new Color(0f, 0f, 0f, darkness);
    }
}
