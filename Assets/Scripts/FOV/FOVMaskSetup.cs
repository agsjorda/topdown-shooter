using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper that renders the FOVMask layer into a RenderTexture and wires it to a fullscreen UI material.
/// Place this on a Camera (or create a dedicated mask camera). Set fovLayerMask to the layers your FOV objects use (e.g. "FOVMask").
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FOVMaskSetup : MonoBehaviour
{
    [Tooltip("Camera used to render the FOV layer. If null the component camera is used.")]
    public Camera maskCamera;

    [Tooltip("Unlit material using FOVDarkness.shader (assign in inspector).")]
    public Material darknessMaterial;

    [Tooltip("Layer name that contains FOV geometry (legacy - kept for backward compatibility).")]
    public string fovLayerName = "FOVMask";

    [Tooltip("Layer mask that contains FOV geometry. Edit this in the inspector to select one or more layers.")]
    public LayerMask fovLayerMask;

    [Tooltip("RenderTexture resolution (square). 256..1024 recommended.")]
    public int maskResolution = 512;

    [Tooltip("If true automatically creates a fullscreen Canvas + RawImage using darknessMaterial.")]
    public bool autoCreateUI = true;

    [Tooltip("If true the mask camera will render every frame in Edit mode. Disable to save perf and call RenderMaskManually().")]
    public bool updateEveryFrameInEditor = true;

    private RenderTexture maskRT;
    private RawImage uiRawImage;

    void OnEnable()
    {
        if (maskCamera == null) maskCamera = GetComponent<Camera>();
        SetupMaskCamera();
        CreateOrFindUI();
        CreateRT();
        AssignRT();
    }

    void OnDisable()
    {
        if (maskCamera != null) maskCamera.targetTexture = null;
        ReleaseRT();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && !updateEveryFrameInEditor) return;
#endif
        // ensure RT matches resolution
        if (maskRT == null || maskRT.width != maskResolution)
            RecreateRT();

        // Ensure maskTexture is kept in material (editor + runtime)
        if (darknessMaterial != null && maskRT != null)
            darknessMaterial.SetTexture("_MaskTex", maskRT);
    }

    private void SetupMaskCamera()
    {
        if (maskCamera == null) return;

        // Prefer the LayerMask editable in the inspector. If it's empty (== 0), fall back to legacy layer name.
        if (fovLayerMask != 0) {
            // LayerMask implicitly converts to int in Unity, assign directly
            maskCamera.cullingMask = fovLayerMask;
        } else {
            int layer = LayerMask.NameToLayer(fovLayerName);
            if (layer >= 0)
                maskCamera.cullingMask = 1 << layer;
        }

        maskCamera.clearFlags = CameraClearFlags.SolidColor;
        maskCamera.backgroundColor = Color.black; // outside FOV = black
        maskCamera.allowHDR = false;
        maskCamera.allowMSAA = false;

        // render to RT (assigned later)
        maskCamera.targetTexture = maskRT;
        // don't show in game view (we use RT)
        maskCamera.enabled = true;
    }

    private void CreateOrFindUI()
    {
        if (!autoCreateUI || darknessMaterial == null) return;

        // try find existing RawImage using this material
        var existing = Object.FindObjectsByType<RawImage>(FindObjectsSortMode.None);
        foreach (var ri in existing) {
            if (ri.material == darknessMaterial) {
                uiRawImage = ri;
                return;
            }
        }

        // create Canvas + RawImage
        var canvasGO = new GameObject("FOVMaskCanvas", typeof(Canvas));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // on top

        var rawGO = new GameObject("FOVMaskImage", typeof(RawImage));
        rawGO.transform.SetParent(canvasGO.transform, false);
        uiRawImage = rawGO.GetComponent<RawImage>();
        uiRawImage.material = darknessMaterial;
        uiRawImage.texture = maskRT;

        // stretch full screen
        var rt = uiRawImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private void CreateRT()
    {
        if (maskResolution < 4) maskResolution = 4;
        ReleaseRT();
        maskRT = RenderTexture.GetTemporary(maskResolution, maskResolution, 0, RenderTextureFormat.R8);
        maskRT.filterMode = FilterMode.Bilinear;
        maskRT.wrapMode = TextureWrapMode.Clamp;

        if (maskCamera != null) maskCamera.targetTexture = maskRT;
        if (darknessMaterial != null) darknessMaterial.SetTexture("_MaskTex", maskRT);
        if (uiRawImage != null) uiRawImage.texture = maskRT;
    }

    private void ReleaseRT()
    {
        if (maskRT != null) {
            RenderTexture.ReleaseTemporary(maskRT);
            maskRT = null;
        }
    }

    private void RecreateRT()
    {
        CreateRT();
        if (maskCamera != null) maskCamera.targetTexture = maskRT;
        if (darknessMaterial != null) darknessMaterial.SetTexture("_MaskTex", maskRT);
        if (uiRawImage != null) uiRawImage.texture = maskRT;
    }

    private void AssignRT()
    {
        if (darknessMaterial != null && maskRT != null)
            darknessMaterial.SetTexture("_MaskTex", maskRT);
        if (uiRawImage != null)
            uiRawImage.texture = maskRT;
    }

    // call this from your FOVPlayer after DrawFOV() if you want to render mask only when the mesh changes:
    public void RenderMaskManually()
    {
        if (maskCamera == null) maskCamera = GetComponent<Camera>();
        if (maskCamera != null && maskRT != null) {
            maskCamera.targetTexture = maskRT;
            maskCamera.Render();
        }
    }
}

/// <summary>
/// Small extension helpers for LayerMask handling (kept local to this file).
/// </summary>
static class LayerMaskExtensions
{
    public static int layerCount(this LayerMask mask)
    {
        int v = mask.value;
        if (v == 0) return 0;
        int count = 0;
        for (int i = 0; i < 32; i++) {
            if ((v & (1 << i)) != 0) count++;
        }
        return count;
    }
}
