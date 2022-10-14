using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    private const float gravityMultiplier = 2;

    public float walkingSpeed = 5;
    public float jumpingSpeed = 10;
    public float highJumpSpeed = 20;

    private bool shouldJump;
    private bool shouldDoHighJump;
    private bool previouslyGrounded;

    private MouseLook mouseLook;

    private Camera targetCamera;
    private Vector2 inputVector;
    private Vector3 movingDirection = Vector3.zero;
    private CharacterController characterController;
    private CollisionFlags collisionFlags;

    private float timeSinceNotGrounded = 0;
    private float timeSincePressedSpace = 0;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        characterController = GetComponent<CharacterController>();
        targetCamera = GetComponentInChildren<Camera>();
        mouseLook = new MouseLook();
        mouseLook.Init(transform, targetCamera.transform);
    }

    private void Update()
    {
        mouseLook.LookRotation(transform, targetCamera.transform);

        if (shouldJump)
        {
            timeSincePressedSpace += Time.deltaTime;
            if (timeSincePressedSpace > 0.05f)
            {
                shouldJump = false;
            }
        }
        else
        {
            shouldJump = Input.GetKeyDown(KeyCode.Space);
            timeSincePressedSpace = 0;
        }

        if (!previouslyGrounded && characterController.isGrounded)
        {
            previouslyGrounded = true;
            timeSinceNotGrounded = 0;
        }

        if (previouslyGrounded && (!characterController.isGrounded))
        {
            timeSinceNotGrounded += Time.deltaTime;
            if (timeSinceNotGrounded > 0.02f)
            {
                previouslyGrounded = false;
            }
        }
    }

    private void FixedUpdate()
    {
        ProcessInput();

        Vector3 desiredMove = transform.forward * inputVector.y + transform.right * inputVector.x;

        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
                           characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        movingDirection.x = desiredMove.x * walkingSpeed;
        movingDirection.z = desiredMove.z * walkingSpeed;

        if (previouslyGrounded || characterController.isGrounded)
        {
            if (shouldJump)
            {
                movingDirection.y = jumpingSpeed;
                shouldJump = false;
            }
        }
        else
        {
            movingDirection += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        if (shouldDoHighJump)
        {
            movingDirection.y = highJumpSpeed;
            shouldDoHighJump = false;
        }

        collisionFlags = characterController.Move(movingDirection * Time.fixedDeltaTime);

        mouseLook.UpdateCursorLock();
    }

    public void DoHighJump()
    {
        shouldDoHighJump = true;
    }

    private void ProcessInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        inputVector = new Vector2(horizontal, vertical);

        if (inputVector.sqrMagnitude > 1)
        {
            inputVector.Normalize();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (collisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}

class MouseLook
{
    private const float MINIMUM_X = -90F;
    private const float MAXIMUM_X = 90F;

    public float sensivity = 2f;

    private Quaternion characterTargetRotation;
    private Quaternion cameraTargetRotation;
    private bool cursorIsLocked = true;

    public void Init(Transform character, Transform camera)
    {
        characterTargetRotation = character.localRotation;
        cameraTargetRotation = camera.localRotation;
    }

    public void LookRotation(Transform character, Transform camera)
    {
        float yRot = Input.GetAxis("Mouse X") * sensivity;
        float xRot = Input.GetAxis("Mouse Y") * sensivity;

        characterTargetRotation *= Quaternion.Euler(0f, yRot, 0f);
        cameraTargetRotation *= Quaternion.Euler(-xRot, 0f, 0f);

        cameraTargetRotation = ClampRotationAroundXAxis(cameraTargetRotation);

        character.localRotation = characterTargetRotation;
        camera.localRotation = cameraTargetRotation;

        UpdateCursorLock();
    }

    public void UpdateCursorLock()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            cursorIsLocked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            cursorIsLocked = true;
        }

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MINIMUM_X, MAXIMUM_X);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

}