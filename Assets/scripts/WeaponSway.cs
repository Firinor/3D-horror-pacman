using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.05f;
    public float maxSwayAmount = 0.1f;
    public float swaySmooth = 6f;

    private Vector3 startPosition;

    private void Start()
    {
        // Запоминаем стартовую позицию (для пустышки это будет 0,0,0)
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        // Получаем движение мыши
        float mouseX = -Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = -Input.GetAxis("Mouse Y") * swayAmount;

        // Ограничиваем, чтобы рука не улетала слишком далеко за экран
        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        // Рассчитываем новую позицию покачивания
        Vector3 targetPosition = new Vector3(mouseX, mouseY, 0);

        // Плавно двигаем объект
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition + targetPosition, Time.deltaTime * swaySmooth);
    }
}