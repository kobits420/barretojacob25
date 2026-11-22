using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class FirstPersonController : MonoBehaviour
{
    [SerializeField] Transform eye;
    [SerializeField] CodeLockPanel lockPanel;
    [SerializeField] float lookSensitivity = 2.2f;
    [SerializeField] float moveSpeed = 3.4f;
    [SerializeField] float gravity = -18f;

    CharacterController controller;
    float yaw;
    float pitch;
    float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        yaw = transform.eulerAngles.y;
        if (eye == null && Camera.main != null)
            eye = Camera.main.transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (lockPanel != null && lockPanel.IsOpen) return;

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Look();
        }

        Move();
    }

    void Look()
    {
        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (eye != null)
            eye.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 wish = transform.right * x + transform.forward * z;
        if (wish.sqrMagnitude > 1f) wish.Normalize();

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = wish * moveSpeed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}
