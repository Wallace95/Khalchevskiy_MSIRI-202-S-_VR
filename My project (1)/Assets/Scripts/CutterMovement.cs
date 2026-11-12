using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutterMovement : MonoBehaviour
{
    [Header("Настройки движения фрезы")]
    public Vector3 moveDirection = Vector3.down;
    public float distance = 0.15f;
    public float speed = 0.15f;

    [Tooltip("Вкл: опустится и поднимется 1 раз. Выкл: двигается до нажатия Stop")]
    public bool oneCycle = true;

    [Header("Настройки текста (UI)")]
    [Tooltip("Перетащите сюда объект с надписью New Text из Hierarchy")]
    public GameObject statusTextObject;
    public string startMessage = "Фреза опускается...";
    public string doneMessage = "Действие выполнено: цикл фрезы завершен";
    public string stopMessage = "Действие выполнено: фреза остановлена";

    private Vector3 startPosition;
    private bool isMoving = false;
    private float timer = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime * speed;

        if (oneCycle && timer >= distance * 2f)
        {
            timer = 0f;
            transform.position = startPosition;
            isMoving = false;
            UpdateStatusText(doneMessage);
            return;
        }

        float offset = Mathf.PingPong(timer, distance);
        transform.position = startPosition + moveDirection.normalized * offset;
    }

    public void StartCutter()
    {
        if (oneCycle && !isMoving)
        {
            timer = 0f;
        }
        isMoving = true;
        UpdateStatusText(startMessage);
    }

    public void StopCutter()
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