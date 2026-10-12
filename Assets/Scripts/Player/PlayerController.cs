
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private InputSystem_Actions inputActions;

    private Vector3 verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();

        if (cameraTransform == null)
        {
            Debug.LogError(
                "Chưa gán Camera Transform cho PlayerController.",
                this
            );
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    private void Update()
    {
        if (cameraTransform == null)
            return;

        MovePlayer();
        LookAround();
    }

    private void MovePlayer()
    {
        Vector2 moveInput =
            inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        characterController.Move(
            moveDirection * moveSpeed * Time.deltaTime
        );

        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        characterController.Move(
            verticalVelocity * Time.deltaTime
        );
    }

    private void LookAround()
    {
        Vector2 lookInput =
            inputActions.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
