using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4.0f;
    public float gravity = -9.81f;

    [Header("Mouse & Gamepad Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2.0f;
    public float gamepadSensitivity = 150.0f;
    public float upDownRange = 80.0f;

    [Header("Flashlight Settings")]
    public Light flashlight;
    public float currentEnergy = 100f;
    public float maxEnergy = 100f;
    public float flashCost = 50f;
    public float flashIntensityMultiplier = 6f;

    [Header("Magnet Settings")]
    public float magnetRadius = 1.5f;
    public float magnetSpeed = 8.0f;

    [Header("Battery Settings")]
    public int batteriesCollected = 0;
    
    [SerializeField] private Transform handTransform; //flashlight_hand_2
    
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float maxSwayAmount = 0.1f;
    [SerializeField] private float swaySmoothing = 4f;
    
    [SerializeField] private float bobSpeed = 12f;
    [SerializeField] private float bobAmountX = 0.12f;
    [SerializeField] private float bobAmountY = 0.08f;
    [SerializeField] private float bobSmoothing = 12f;
    
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float checkDistance = 0.8f;
    [SerializeField] private Vector3 pushBackOffset = new Vector3(-0.05f, -0.05f, -0.2f);
    [SerializeField] private float pushBackSpeed = 5f;
    
    [SerializeField] private float strobeDuration = 0.5f;
    [SerializeField] private float flashInterval = 0.05f;
    [SerializeField] private float attackCooldown = 1.2f;
    
    [SerializeField] private float kickBackForce = 0.15f;
    [SerializeField] private float kickUpForce = 0.05f;
    [SerializeField] private float returnSpeed = 10f;
    
    private CharacterController characterController;
    private float verticalRotation = 0f;
    private float verticalVelocity = 0f;
    private float originalIntensity;
    private bool isFlashing = false;
    private float flashTimer = 0f;

    private Vector3 handOriginPos;
    private float bobTimer = 0f;
    private float currentPushFactor = 0f;
    private bool isStrobeAttacking = false;
    private float nextAttackTime = 0f;
    private Vector3 currentRecoilOffset = Vector3.zero;

    private Vector3 smoothedBobOffset = Vector3.zero;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (flashlight != null)
        {
            originalIntensity = flashlight.intensity;
            flashlight.enabled = true;
        }

        if (handTransform != null)
        {
            handOriginPos = handTransform.localPosition;
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleGameplayMechanics();
        HandleHandAnimations();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        //float gamepadX = Input.GetAxis("Gamepad X") * gamepadSensitivity * Time.deltaTime;
        //float gamepadY = Input.GetAxis("Gamepad Y") * gamepadSensitivity * Time.deltaTime;

        float totalX = mouseX;// + gamepadX;
        float totalY = mouseY;// + gamepadY;
        
        transform.Rotate(Vector3.up * totalX);
        
        verticalRotation -= totalY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleGameplayMechanics()
    {
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                isFlashing = false;
                if (flashlight != null)
                {
                    flashlight.intensity = originalIntensity;
                }
            }
        }

        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime && !isStrobeAttacking && !isFlashing)
        {
            if (currentEnergy >= flashCost)
            {
                currentEnergy -= flashCost;
                StartCoroutine(PlayStrobeAttack());
            }
            else
            {
                //Debug.Log("currentEnergy: " + currentEnergy);
            }
        }

        Battery[] batteries = FindObjectsByType<Battery>(FindObjectsSortMode.None);
        foreach (Battery battery in batteries)
        {
            if (battery != null)
            {
                float distance = Vector3.Distance(transform.position, battery.transform.position);

                if (distance <= magnetRadius)
                {
                    Vector3 targetPosition = transform.position;
                    targetPosition.y += 0.4f;

                    battery.transform.position = Vector3.MoveTowards(battery.transform.position, targetPosition, magnetSpeed * Time.deltaTime);
                }
            }
        }
    }

    public void CollectBattery()
    {
        batteriesCollected++;
        currentEnergy = Mathf.Min(currentEnergy + 5f, maxEnergy);
    }

    private void HandleHandAnimations()
    {
        if (handTransform == null) return;

        float totalInputX = Input.GetAxis("Mouse X");// + (Input.GetAxis("Gamepad X") * 5f * Time.deltaTime);
        float totalInputY = Input.GetAxis("Mouse Y");// + (Input.GetAxis("Gamepad Y") * 5f * Time.deltaTime);

        float movementX = -totalInputX * swayAmount;
        float movementY = -totalInputY * swayAmount;

        movementX = Mathf.Clamp(movementX, -maxSwayAmount, maxSwayAmount);
        movementY = Mathf.Clamp(movementY, -maxSwayAmount, maxSwayAmount);

        Vector3 targetSwayPosition = new Vector3(movementX, movementY, 0f);
        
        Vector3 targetBobbingPosition = Vector3.zero;
        float horizontalSpeed = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;

        if (horizontalSpeed > 0.1f && characterController.isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            targetBobbingPosition.x = Mathf.Sin(bobTimer) * bobAmountX;
            targetBobbingPosition.y = Mathf.Cos(bobTimer * 2f) * bobAmountY;
        }
        else
        {
            bobTimer = Mathf.MoveTowards(bobTimer, 0f, Time.deltaTime * bobSpeed);
        }

        smoothedBobOffset = Vector3.Lerp(smoothedBobOffset, targetBobbingPosition, Time.deltaTime * bobSmoothing);

        //(Push Back)
        float targetPushFactor = 0f;
        if (cameraTransform != null)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, checkDistance, wallLayer))
            {
                targetPushFactor = 1f - (hit.distance / checkDistance);
            }
        }

        currentPushFactor = Mathf.MoveTowards(currentPushFactor, targetPushFactor, Time.deltaTime * pushBackSpeed);
        Vector3 targetPushOffset = pushBackOffset * currentPushFactor;
        
        currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, Vector3.zero, Time.deltaTime * returnSpeed);
        
        Vector3 finalTargetPosition = handOriginPos + targetSwayPosition + smoothedBobOffset + targetPushOffset + currentRecoilOffset;
        handTransform.localPosition = Vector3.Lerp(handTransform.localPosition, finalTargetPosition, Time.deltaTime * swaySmoothing);
    }

    private IEnumerator PlayStrobeAttack()
    {
        isStrobeAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        currentRecoilOffset = new Vector3(0f, kickUpForce, -kickBackForce);

        float elapsed = 0f;
        bool lightState = false;

        while (elapsed < strobeDuration)
        {
            if (flashlight != null)
            {
                flashlight.enabled = lightState;
            }

            lightState = !lightState;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (flashlight != null)
        {
            flashlight.enabled = true;
            flashlight.intensity = originalIntensity;
        }

        isStrobeAttacking = false;
    }
}