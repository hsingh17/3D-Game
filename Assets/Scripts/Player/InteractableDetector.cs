using UnityEngine;

public class InteractableDetector : MonoBehaviour
{
    [SerializeField]
    private LayerMask interactableLayer;

    [SerializeField]
    [Range(0, 10f)]
    private float maxInteractDistance;

    [SerializeField]
    private Camera camera;

    private void FixedUpdate()
    {
        // TODO: Put in it's own func
        Debug.DrawRay(camera.transform.position, camera.transform.forward);
        bool hit = Physics.Raycast(
            camera.transform.position,
            camera.transform.forward,
            out RaycastHit info,
            maxInteractDistance,
            interactableLayer
        );

        if (hit)
        {
            Debug.Log(info.transform.gameObject);
        }
    }
}
