using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform BatteryPool;
    public Enemy.Biha_Enemy Enemy;
    
    [Header("Movement Settings")]
    public float moveSpeed = 4.0f;
    public float runSpeed = 12.0f;
    
    public float sprintRegen = 0.2f;
    public float sprintPrice = 0.1f;
    public float sprintRecoveryDelay = 2f;
    private float sprintRecoveryTimer;
    
    public float gravity = -9.81f;

    [Header("Mouse & Gamepad Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2.0f;
    public float gamepadSensitivity = 150.0f;
    public float upDownRange = 89.0f;

    [Header("Flashlight Settings")]
    public Light flashlight;
    public float flashlightAngle = 120;
    public float flashlightFocusSpeed = 3f;
    public float maxEnergy = 100f;
    public float flashCost = 50f;
    public Image FlashCostReadyImage;
    private float flashIntensityMultiplierTimer;
    
    [Header("Magnet Settings")]
    public float magnetRadius = 1.5f;
    public float magnetSpeed = 8.0f;
    
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
    
    [Header("Audio")]
    public AudioSource hartbeat;
    public AudioSource hartbeatPanic;
    
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

    public event Action OnBatteryPick;
    public event Action OnKeyPick;
    
    //View
    public Slider StaminaSlider;
    public Slider FlashlightSlider;

    private bool isCanMove = true;
    private PlayerInputHolder input;
    private InputActionAsset action;
    private InputAction MoveAction;
    private InputAction SprintAction;
    
    private struct PlayerInputHolder
    {
        public Vector2 move;
        public Vector2 look;
        public bool fire;
        public bool sprint;
    }
    
    void Start()
    {
        FlashlightSlider.maxValue = maxEnergy;
        FlashlightSlider.value = maxEnergy;
        FlashCostReadyImage.enabled = FlashlightSlider.value >= flashCost;
        
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

        InputSubscribe();
    }

    private void InputSubscribe()
    {
        action = InputSystem.actions;
        //EnhancedTouchSupport.Enable();
        action.FindAction("Look").performed += LookInput;
        action.FindAction("Attack").performed += AttackInput;
        MoveAction = action.FindAction("Move");
        SprintAction = action.FindAction("Sprint");
    }

    private void AttackInput(InputAction.CallbackContext obj)
    {
        input.fire = obj.ReadValue<float>() > 0;
    }
    
    private void LookInput(InputAction.CallbackContext obj)
    {
        input.look = obj.ReadValue<Vector2>() * mouseSensitivity;
    }

    void Update()
    {
        if (isCanMove)
        {
            input.move = MoveAction.ReadValue<Vector2>();
            input.sprint = SprintAction.ReadValue<float>() > 0 
                && input.move.magnitude > 0f;
        }
        
        HandleMouseLook();
        HandleFlashlight();
        HandleStamina();
        HandleMovement();
        HandleGameplayMechanics();
        HandleHandAnimations();
        HandleAudio();

        input.sprint = false;
        input.fire = false;
        input.move = Vector2.zero;
        input.look = Vector2.zero;
    }

    private void HandleAudio()
    {
        
    }

    private void HandleFlashlight()
    {
        Vector3 target = GetNearestTarget();
        Vector3 directionToTarget = (target - transform.position).normalized;
        Vector3 cameraForward = transform.forward;
        float angle = Vector3.Angle(cameraForward, directionToTarget);
        float factor = angle / 180f;

        float targetAngle = flashlightAngle / 4;
        float deltaIn = targetAngle * factor;
        float deltaOut = targetAngle * 3 * factor;
        flashlight.innerSpotAngle = Mathf.Lerp(flashlight.innerSpotAngle, targetAngle - deltaIn, Time.deltaTime * flashlightFocusSpeed);
        flashlight.spotAngle = Mathf.Lerp(flashlight.spotAngle, targetAngle + deltaOut, Time.deltaTime * flashlightFocusSpeed);
    }

    private Vector3 GetNearestTarget()
    {
        Vector3 closest = new Vector3(-999, 0, 0);
        float minDistance = float.MaxValue;
    
        for(int i = 0; i < BatteryPool.childCount; i++)
        {
            Transform target = BatteryPool.GetChild(i);
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target.position;
            }
        }
    
        return closest;
    }

    private void HandleStamina()
    {
        if (input.sprint && StaminaSlider.value > 0)
        {
            sprintRecoveryTimer = sprintRecoveryDelay;
            StaminaSlider.value -= sprintPrice * Time.deltaTime;
        }
        else
        {
            sprintRecoveryTimer -= Time.deltaTime;
        }
        
        if (sprintRecoveryTimer <= 0)
            StaminaSlider.value += sprintRegen * Time.deltaTime;
    }

    private void HandleMouseLook()
    {
        transform.Rotate(Vector3.up * input.look.x);
        
        verticalRotation -= input.look.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        Vector3 move = transform.right * input.move.x + transform.forward * input.move.y;

        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;

        float speed = moveSpeed;
        
        if (input.sprint && StaminaSlider.value > 0)
        {
            speed = runSpeed;
        }
        
        characterController.Move(move * speed * Time.deltaTime);
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

        if (input.fire
            && FlashlightSlider.value >= flashCost
            && Time.time >= nextAttackTime 
            && !isStrobeAttacking 
            && !isFlashing)
        {
            FlashlightSlider.value -= flashCost;
            
            FlashCostReadyImage.enabled = FlashlightSlider.value >= flashCost;
            
            StartCoroutine(PlayStrobeAttack());
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

    public void CollectBattery(string key)
    {
        switch (key)
        {
            case "Battery":
                CollectBattery();
                break;
            case "Key":
                CollectKey();
                break;
            default:
                Debug.LogError("What the key?");
                break;
        }
    }

    private void CollectKey()
    {
        OnKeyPick?.Invoke();
    }
    private void CollectBattery()
    {
        FlashlightSlider.value += 5f;
        
        FlashCostReadyImage.enabled = FlashlightSlider.value >= flashCost;
        
        SoundManager.Instance.PlayBatteryCollect(transform.position);
        OnBatteryPick?.Invoke();
    }

    private void HandleHandAnimations()
    {
        if (handTransform == null) return;

        float totalInputX = input.move.x;
        float totalInputY = input.move.y;

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

        SoundManager.Instance.PlayFlashlightAttack();
        currentRecoilOffset = new Vector3(0f, kickUpForce, -kickBackForce);

        float elapsed = 0f;
        bool lightState = false;

        while (elapsed < strobeDuration)
        {
            flashlight.enabled = lightState;

            lightState = !lightState;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        Enemy.ToBunishment();
        
        flashlight.enabled = true;
        flashlight.intensity = originalIntensity;

        isStrobeAttacking = false;
    }
    
    public void ToParalyze()
    {
        isCanMove = false;
    }

    private void OnDestroy()
    {
        action.FindAction("Look").performed -= LookInput;
        action.FindAction("Attack").performed -= AttackInput;
    }
}