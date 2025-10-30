using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private CinemachineCamera cinemachineCamera;
    private CinemachinePositionComposer positionComposer;


    [Header("Camera Distance Settings")]
    [SerializeField] private bool canChangeCameraDistance;
    [SerializeField] private float distanceChangeRate;
    private float targetCameraDistance;


    private void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Debug.LogWarning("Multiple instances of CameraManager detected. Destroying duplicate.");
            Destroy(gameObject);
        }

        cinemachineCamera = GetComponentInChildren<CinemachineCamera>();

        positionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
    }

    private void Update()
    {
        UpdateCameraDistance();

    }

    private void UpdateCameraDistance()
    {
        if (canChangeCameraDistance == false) return;


        float curentDistance = positionComposer.CameraDistance;

        if (Mathf.Abs(targetCameraDistance - curentDistance) < 0.01f)
            return;

        positionComposer.CameraDistance = Mathf.Lerp(positionComposer.CameraDistance, targetCameraDistance, distanceChangeRate * Time.deltaTime);
    }

    public void ChangeCameraDistance(float distance) => targetCameraDistance = distance;

}
