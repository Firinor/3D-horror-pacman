using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.05f;
    public float maxSwayAmount = 0.1f;
    public float swaySmooth = 6f;

    private Vector3 startPosition;
    
    private InputActionAsset action;
    private InputAction MoveAction;

    private Vector2 mouseLook;

    private void Start()
    {
        action = InputSystem.actions;
        action.FindAction("Look").performed += LookInput;
        
        startPosition = transform.localPosition;
    }

    private void LookInput(InputAction.CallbackContext obj)
    {
        mouseLook = obj.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        float mouseX = -mouseLook.x * swayAmount;
        float mouseY = -mouseLook.y * swayAmount;
        
        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);
        
        Vector3 targetPosition = new Vector3(mouseX, mouseY, 0);
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition + targetPosition, Time.deltaTime * swaySmooth);
    }

    private void OnDestroy()
    {
        action.FindAction("Look").performed -= LookInput;
    }
}