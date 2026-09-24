using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        HideCursor();

        // TODO: Camera cull out 3rd person view of player model
        // camera.cullingMask;
    }

    private void FixedUpdate()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        // Vector3 curRotationEulerAngles = camera.transform.rotation.eulerAngles;
        // pitch = Mathf.Clamp(pitch + (-look.y * mouseSensitivity.y), pitchMin, pitchMax);
        // camera.transform.rotation = Quaternion.Euler(
        //     pitch,
        //     curRotationEulerAngles.y,
        //     curRotationEulerAngles.z
        // );
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
