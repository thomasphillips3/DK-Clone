using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person studio explorer. Walk only — no run, no jump.
/// Optional subtle camera bob synced to movement.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class StudioExplorer : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Camera Bob")]
    [SerializeField] private bool enableCameraBob = true;
    [SerializeField] private float bobFrequency = 1.8f;
    [SerializeField] private float bobAmplitude = 0.03f;

    private CharacterController controller;
    private Transform cameraTransform;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation;
    private float yVelocity;
    private float bobTimer;
    private float defaultCameraY;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>()?.transform;
        if (cameraTransform != null)
            defaultCameraY = cameraTransform.localPosition.y;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        if (enableCameraBob)
            HandleBob();
    }

    void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMove()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= walkSpeed;

        if (controller.isGrounded && yVelocity < 0f)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;
        move.y = yVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void HandleBob()
    {
        if (cameraTransform == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f && controller.isGrounded;
        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float bobOffset = Mathf.Sin(bobTimer * Mathf.PI * 2f) * bobAmplitude;
            Vector3 pos = cameraTransform.localPosition;
            pos.y = defaultCameraY + bobOffset;
            cameraTransform.localPosition = pos;
        }
        else
        {
            bobTimer = 0f;
            Vector3 pos = cameraTransform.localPosition;
            pos.y = Mathf.Lerp(pos.y, defaultCameraY, Time.deltaTime * 8f);
            cameraTransform.localPosition = pos;
        }
    }

    // Input System callbacks
    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();

    public void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            bool isPaused = Time.timeScale < 0.5f;
            if (isPaused)
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                AlbumAudioManager.Instance?.Resume();
            }
            else
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                AlbumAudioManager.Instance?.Pause();
            }
        }
    }

    public void SetSensitivity(float sens) => mouseSensitivity = sens;
    public void SetCameraBob(bool enabled) => enableCameraBob = enabled;
}
