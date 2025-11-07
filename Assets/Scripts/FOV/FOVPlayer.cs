using UnityEngine;

/// <summary>
/// Draws a field of view (FOV) mesh that follows the player's facing direction.
/// Used as a mask to reveal light inside darkness.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FOVPlayer : MonoBehaviour
{
    [Header("FOV Settings")]
    [Range(10f, 360f)] public float fovAngle = 90f;
    [Range(1f, 20f)] public float viewDistance = 8f;
    [Range(10, 500)] public int rayCount = 100;

    [Header("Height Settings")]
    public float fovOriginHeight = 1.6f;
    public float fovHeight = 0.5f;

    [Header("Detection Layers")]
    public LayerMask obstacleMask;
    public LayerMask enemyMask;

    [Header("Visual Settings")]
    [Tooltip("Color and transparency of the FOV mesh. Updates live in Play mode.")]
    public Color fovColor = new Color(1f, 1f, 1f, 0.5f);

    [Tooltip("Material used to render the FOV cone. Must be assigned manually in the Inspector.")]
    public Material fovMaterial;

    [Tooltip("If true, the FOV mesh will be visible and opaque. If false the cone will be rendered with the configured transparency.")]
    public bool showVisual = false;

    [Header("Performance Settings")]
    public int updateEveryNFrames = 2;
    public bool showDebugRays = false;

    private Mesh mesh;
    private int frameCounter;
    private MeshRenderer meshRenderer;
    private Color lastAppliedColor;
    private bool lastShowVisual;

    void Awake()
    {
        mesh = new Mesh { name = "FOV Mesh" };
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisualState(force: true);
    }

    void OnValidate()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (mesh != null)
            mesh.MarkDynamic();

        UpdateVisualState(force: true);
    }

    void LateUpdate()
    {
        frameCounter++;
        if (frameCounter % Mathf.Max(1, updateEveryNFrames) == 0)
            DrawFOV();

        UpdateEnemyVisibility();
        UpdateVisualState(); // check for runtime inspector updates
    }

    /// <summary>
    /// Updates MeshRenderer visibility and applies color changes live.
    /// When showVisual is true the cone is forced opaque (alpha = 1).
    /// When showVisual is false the cone uses the configured fovColor alpha (transparent).
    /// </summary>
    private void UpdateVisualState(bool force = false)
    {
        if (meshRenderer == null) return;

        // If no material, disable renderer entirely
        if (fovMaterial == null) {
            meshRenderer.enabled = false;
            return;
        }

        // Ensure the renderer uses the configured material
        if (force || meshRenderer.sharedMaterial != fovMaterial) {
            meshRenderer.sharedMaterial = fovMaterial;
        }

        // Show the cone in the Scene/Game view when showVisual is checked.
        // When unchecked the renderer is disabled (so cone is not visible).
        meshRenderer.enabled = showVisual;

        // Choose alpha: opaque when showVisual is true, otherwise use fovColor.a
        Color applied = fovColor;
        applied.a = showVisual ? 1f : fovColor.a;

        // Apply color only when it changed or forced
        if (force || applied != lastAppliedColor) {
            ApplyMaterialColor(fovMaterial, applied);
            lastAppliedColor = applied;
        }

        lastShowVisual = showVisual;
    }

    /// <summary>
    /// Applies the given color to the material if it has a known color property.
    /// Note: this modifies the assigned material's color property (sharedMaterial).
    /// If you want per-instance non-destructive changes use MaterialPropertyBlock instead.
    /// </summary>
    private void ApplyMaterialColor(Material mat, Color color)
    {
        if (mat == null) return;

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        else if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else if (mat.HasProperty("_TintColor"))
            mat.SetColor("_TintColor", color);
    }

    private void DrawFOV()
    {
        Vector3 origin = transform.position + Vector3.up * fovOriginHeight;
        float angle = -fovAngle / 2f;
        float step = fovAngle / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= rayCount; i++) {
            Vector3 dir = DirFromAngle(angle);
            Vector3 worldDir = transform.TransformDirection(dir);
            Vector3 hitPoint = origin + worldDir * viewDistance;

            if (Physics.Raycast(origin, worldDir, out RaycastHit hit, viewDistance, obstacleMask))
                hitPoint = hit.point;

            if (showDebugRays)
                Debug.DrawLine(origin, hitPoint, Color.yellow);

            vertices[i + 1] = transform.InverseTransformPoint(hitPoint);

            if (i > 0) {
                int tri = (i - 1) * 3;
                triangles[tri] = 0;
                triangles[tri + 1] = i;
                triangles[tri + 2] = i + 1;
            }

            angle += step;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void UpdateEnemyVisibility()
    {
        Vector3 origin = transform.position + Vector3.up * fovOriginHeight;
        Collider[] hits = Physics.OverlapSphere(origin, viewDistance, enemyMask);

        foreach (var col in hits) {
            if (col == null) continue;
            GameObject enemyRoot = col.transform.root.gameObject;

            Vector3 target = enemyRoot.transform.position;
            float dist = Vector3.Distance(origin, target);

            // Height & angle check
            if (Mathf.Abs(target.y - origin.y) > fovHeight * 0.5f) {
                SetEnemyVisible(enemyRoot, false);
                continue;
            }

            Vector3 dirToEnemy = (target - origin).normalized;
            float angleToEnemy = Vector3.Angle(transform.forward, new Vector3(dirToEnemy.x, 0, dirToEnemy.z));
            if (angleToEnemy > fovAngle / 2f) {
                SetEnemyVisible(enemyRoot, false);
                continue;
            }

            if (Physics.Raycast(origin, dirToEnemy, out RaycastHit hit, dist, obstacleMask)) {
                if (hit.collider.transform.root != enemyRoot.transform) {
                    SetEnemyVisible(enemyRoot, false);
                    continue;
                }
            }

            SetEnemyVisible(enemyRoot, true);
        }
    }

    private void SetEnemyVisible(GameObject enemy, bool visible)
    {
        if (!enemy) return;
        var renderers = enemy.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var r in renderers)
            r.enabled = visible;
    }

    private Vector3 DirFromAngle(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * fovOriginHeight, viewDistance);
    }
}
