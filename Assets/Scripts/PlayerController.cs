using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float turnSpeed = 180.0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float mouseSensitivity = 3.0f;

    private CharacterController controller;
    private new Camera camera;
    private float cameraAngle;
    private Vector3 velocity;
    private bool isJumping = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        camera = GetComponentInChildren<Camera>();
        cameraAngle = camera.transform.localEulerAngles.x;
    }

    void Update()
    {
        SetVelocityFromInput();
        AddVelocityFromGravity();
        controller.Move(velocity * Time.deltaTime);
        SetRotation();
        SetCameraAngle();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    void SetVelocityFromInput()
    {
        Vector3 walkVelocity = transform.forward * Input.GetAxis("Walk") * walkSpeed;
        float strafeInput = Input.GetAxis("Strafe");
        if (CtrlPressed())
        {
            strafeInput = Input.GetAxis("Turn");
        }
        Vector3 strafeVelocity = transform.right * strafeInput * walkSpeed;
        Vector3 inputVelocity = walkVelocity + strafeVelocity;
        velocity.x = inputVelocity.x;
        velocity.z = inputVelocity.z;
    }

    void AddVelocityFromGravity()
    {
        float gravitationalAcceleration = 9.81f;
        if (controller.isGrounded && !isJumping)
        {
            velocity.y = -1.0f;  // Small negative value to keep the player "attached" to the ground, so that controller.isGrounded remains true. (It would quickly flicker between true and false without this, or if velocity.y was set to 0.0f.)
        }
        else
        {
            isJumping = false;
            velocity.y -= gravitationalAcceleration * Time.deltaTime; // This will keep increasing (in absolute value) until the player stops falling.
        }
    }

    void SetRotation()
    {
        if (!CtrlPressed())
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Turn") * turnSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            float mouseRotationY = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseRotationY);
        }
    }

    void SetCameraAngle()
    {
        if (Input.GetKey(KeyCode.PageUp))
        {
            cameraAngle -= turnSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            cameraAngle += turnSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Home))
        {
            cameraAngle = 0.0f;
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            float mouseRotationX = Input.GetAxis("Mouse Y") * mouseSensitivity;
            cameraAngle -= mouseRotationX;
        }


        cameraAngle = Mathf.Clamp(cameraAngle, -90.0f, 90.0f);
        camera.transform.localEulerAngles = new Vector3(cameraAngle, 0.0f, 0.0f);
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            isJumping = true;
            velocity.y = jumpForce;
        }
    }

    bool CtrlPressed()
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }
}
