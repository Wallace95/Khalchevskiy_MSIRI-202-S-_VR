using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TableMovement : MonoBehaviour
{
    [Header("Настройки движения стола")]
    public Vector3 moveDirection = Vector3.forward;
    public float distance = 0.3f;
    public float speed = 0.2f;

    [Tooltip("Вкл: проедет вперед-назад 1 раз. Выкл: двигается до нажатия Stop")]
    public bool oneCycle = false;

    [Header("Настройки текста (UI)")]
    [Tooltip("Перетащите сюда объект с надписью New Text из Hierarchy")]
    public GameObject statusTextObject;
    public string startMessage = "Действие выполнено: стол движется";
    public string stopMessage = "Действие выполнено: стол остановлен";

    private Vector3 startLocalPosition;
    private bool isMoving = false;
    private float timer = 0f;

    void Start()
    {
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime * speed;

        if (oneCycle && timer >= distance * 2f)
        {
            timer = 0f;
            transform.localPosition = startLocalPosition;
            isMoving = false;
            UpdateStatusText(stopMessage);
            return;
        }

        float offset = Mathf.PingPong(timer, distance);
        transform.localPosition = startLocalPosition + moveDirection.normalized * offset;
    }

    public void StartMove()
    {
        if (oneCycle && !isMoving)
        {
            timer = 0f;
        }
        isMoving = true;
        UpdateStatusText(startMessage);
    }

    public void StopMove()
    {
        isMoving = false;
        UpdateStatusText(stopMessage);
    }

    private void UpdateStatusText(string message)
    {
        if (statusTextObject == null) return;

        TMP_Text tmp = statusTextObject.GetComponent<TMP_Text>();
        if (tmp != null) tmp.text = message;

        Text uiText = statusTextObject.GetComponent<Text>();
        if (uiText != null) uiText.text = message;
    }
}