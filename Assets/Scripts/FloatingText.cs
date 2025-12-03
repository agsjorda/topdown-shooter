
using System.Collections;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class FloatingText : PooledObject
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float lifetime = 1.6f;
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, -1f, 0f); // changed to go down by default

    [Header("Scale animation")]
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float peakScale = 1.15f;
    [SerializeField] private float endScale = 0.6f;

    // changed: explicit grow time in seconds (was confusing fractional 'pulseDuration')
    [SerializeField, Tooltip("Time in seconds for the initial grow phase (clamped to lifetime)")]
    private float growTime = 0.15f;

    [Header("Random appearance")]
    [SerializeField, Range(0f, 20f)] private float maxYawOffset = 8f;
    [SerializeField, Range(0f, 50f)] private float maxRollOffset = 6f;

    private Coroutine running;
    private Quaternion randomOffset;
    private Camera cam;
    private Vector3 instanceMoveOffset; // can be overridden per Show call

    // call this after spawn to set text and start animation
    // optional overrideOffset lets you decide per-instance whether it moves up or down
    public void Show(string text, Vector3? overrideOffset = null)
    {
        if (textMesh != null) {
            textMesh.text = text;
            var c = textMesh.color;
            c.a = 1f;
            textMesh.color = c;
        }

        // per-instance move offset (use prefab default if not provided)
        instanceMoveOffset = overrideOffset ?? moveOffset;

        // choose a small random orientation offset (applied after facing camera)
        GenerateRandomOffset();

        // cache camera reference (safe fallback in case Camera.main is null)
        cam = Camera.main ?? (Camera.current != null ? Camera.current : Camera.main);

        // set initial scale
        transform.localScale = Vector3.one * startScale;

        if (running != null)
            StopCoroutine(running);

        running = StartCoroutine(AnimateAndReturn());
    }

    private IEnumerator AnimateAndReturn()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + instanceMoveOffset;

        // clamp growTime so it never exceeds lifetime
        float grow = Mathf.Clamp(growTime, 0f, lifetime);
        float shrinkDuration = Mathf.Max(0.0001f, lifetime - grow); // avoid divide by zero

        while (elapsed < lifetime) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);

            // position
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            // facing camera + small random offset
            if (cam != null) {
                Vector3 dir = transform.position - cam.transform.position;
                if (dir.sqrMagnitude > 0.0001f) {
                    transform.rotation = Quaternion.LookRotation(dir) * randomOffset;
                }
            }

            // scale: explicit grow (seconds) then shrink (seconds)
            float scale;
            if (elapsed <= grow && grow > 0f) {
                float p = elapsed / grow; // 0..1 over grow seconds
                scale = Mathf.Lerp(startScale, peakScale, Mathf.SmoothStep(0f, 1f, p));
            } else {
                float p = (elapsed - grow) / shrinkDuration; // 0..1 over remaining time
                scale = Mathf.Lerp(peakScale, endScale, p);
            }
            transform.localScale = Vector3.one * scale;

            // fade alpha (fade out over lifetime)
            if (textMesh != null) {
                Color c = textMesh.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                textMesh.color = c;
            }

            yield return null;
        }

        // return to pool if pooled, otherwise destroy
        var pooled = GetComponent<PooledObject>();
        if (pooled != null && ObjectPool.instance != null) {
            ObjectPool.instance.ReturnObject(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // called by ObjectPool.ResetPooledObjectState
    public override void ResetState()
    {
        if (running != null) {
            StopCoroutine(running);
            running = null;
        }

        if (textMesh != null) {
            textMesh.text = "";
            var c = textMesh.color;
            c.a = 1f;
            textMesh.color = c;
        }

        // reset transform state
        transform.localScale = Vector3.one * startScale;
        randomOffset = Quaternion.identity;
        instanceMoveOffset = moveOffset;
    }

    // called in editor when you change values in the inspector (also runs in playmode)
    private void OnValidate()
    {
        // Keep sensible ranges
        growTime = Mathf.Clamp(growTime, 0f, 10f);
        lifetime = Mathf.Max(0.01f, lifetime);

        // Update cached movement offset so active instances respond to inspector changes
        instanceMoveOffset = moveOffset;

        // If an instance is running (in playmode) update its random offset so you see changes immediately
        if (running != null) {
            GenerateRandomOffset();
        }

        // Update scale immediately in editor/play when value is changed
        if (!Application.isPlaying) {
            transform.localScale = Vector3.one * startScale;
        } else if (running == null) {
            // if not running but in playmode, reflect the start scale immediately
            transform.localScale = Vector3.one * startScale;
        }
    }

    // helper - generates a new random orientation offset based on inspector ranges
    private void GenerateRandomOffset()
    {
        float yaw = Random.Range(-maxYawOffset, maxYawOffset);
        float roll = Random.Range(-maxRollOffset, maxRollOffset);
        randomOffset = Quaternion.Euler(0f, yaw, roll);
    }
}