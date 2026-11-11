using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [Header("Настройки скорости")]
    public float normalSpeed = 10f; // Обычная скорость передвижения
    public float fastSpeed = 25f;   // Скорость при зажатом Shift
    public float mouseSensitivity = 3f; // Чувствительность мыши

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // Запоминаем текущий поворот камеры при старте
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
    }

    void Update()
    {
        // Вращение камерой происходит при зажатой правой кнопке мыши
        if (Input.GetMouseButton(1))
        {
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Ограничение по вертикали

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0.0f);
        }

        // Выбор скорости (если зажат Shift, летим быстрее)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : normalSpeed;

        // Перемещение по WASD + Q (вниз) и E (вверх)
        float moveForwardBack = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;   // W / S
        float moveLeftRight = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime; // A / D
        float moveUpDown = 0.0f;

        if (Input.GetKey(KeyCode.E)) moveUpDown += currentSpeed * Time.deltaTime; // Подъем вверх
        if (Input.GetKey(KeyCode.Q)) moveUpDown -= currentSpeed * Time.deltaTime; // Спуск вниз

        transform.Translate(moveLeftRight, moveUpDown, moveForwardBack);
    }
}
