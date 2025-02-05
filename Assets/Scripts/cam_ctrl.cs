using Ragdoll;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Public fields for configuration
    public Transform player;
    public float sensitivity = 100f;
    public float clampAngle = 80f;
    public float raycastDistance = 50f;
    public float hitImpulse = 1000f;

    void Start()
    {
        // Cache the transform for performance
        transform.position = player.position;
    }

    void Update()
    {
        HandleCameraRotation();
        HandleShootingMechanics();
        HandleEscapeKey();
    }

    /// <summary>
    /// Handles camera rotation based on mouse input.
    /// </summary>
    void HandleCameraRotation()
    {
        if (Input.GetMouseButton(1))
        {
            LockCursor(true);

            float mouse_x = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float mouse_y = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            Vector3 rotation = transform.localEulerAngles;
            rotation.x = Mathf.DeltaAngle(0, rotation.x) - mouse_y;
            rotation.x = Mathf.Clamp(rotation.x, -clampAngle, clampAngle);
            rotation.y += mouse_x;
            transform.localEulerAngles = rotation;
        }
        else
        {
            LockCursor(false);
        }

        // Ensure the camera follows the player
        transform.position = player.position;
    }

    /// <summary>
    /// Handles shooting mechanics when the left mouse button is pressed.
    /// </summary>
    void HandleShootingMechanics()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, LayerMask.GetMask("Ragdoll")))
            {
                RagdollBone boneController = hit.collider.GetComponent<RagdollBone>();
                if (boneController != null)
                {
                    HitInfo hitInfo = new HitInfo
                    {
                        bone = boneController,
                        impulse = hitImpulse,
                        hit_point = hit.point,
                        hit_direction = ray.direction
                    };

                    boneController.HitBone(hitInfo);
                    hit.rigidbody.AddForceAtPosition(ray.direction * (hitImpulse * 60), hit.point);
                }
            }
        }
    }

    /// <summary>
    /// Handles quitting the application when the Escape key is pressed.
    /// </summary>
    void HandleEscapeKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// Toggles cursor locking and visibility.
    /// </summary>
    /// <param name="lockCursor">Whether to lock the cursor.</param>
    void LockCursor(bool lockCursor)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
}