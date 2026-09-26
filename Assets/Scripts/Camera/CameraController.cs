using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField]
    private InputReader inputReader;

    [SerializeField]
    [Range(-90f, 90f)]
    private float pitchMin;

    [SerializeField]
    [Range(-90f, 90f)]
    private float pitchMax;

    [SerializeField]
    private LayerMask thirdPersonModelLayer;

    private Camera camera;
    private float pitch;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        HideCursor();

        // Cull out the 3rd person player model from FPS Camera view
        camera.cullingMask ^= thirdPersonModelLayer.value;
    }

    private void FixedUpdate()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        Vector3 curRotationEulerAngles = camera.transform.rotation.eulerAngles;
        pitch = Mathf.Clamp(
            pitch + (-inputReader.Look.y * GameSettings.Instance().Sensitivity.y),
            pitchMin,
            pitchMax
        );

        camera.transform.rotation = Quaternion.Euler(
            pitch,
            curRotationEulerAngles.y,
            curRotationEulerAngles.z
        );
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
