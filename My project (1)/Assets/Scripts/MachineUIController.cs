using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class MachineUIController : MonoBehaviour
{
    [Header("Настройки сервера")]
    [SerializeField] private string serverUrl = "https://cnc.kovalev.team/get/3";
    [SerializeField] private int channel = 1;
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Шпиндель сверлильного станка (ось Z)")]
    [SerializeField] private Transform spindleZ; // Объект Cylinder.001
    [SerializeField] private float zScale = 0.001f; // Коэффициент масштаба движения

    [Header("Текстовые поля (Canvas)")]
    [SerializeField] private TextMeshProUGUI stateText;  // Верхняя строка (information)
    [SerializeField] private TextMeshProUGUI coordsText; // Средняя строка (information 1)
    [SerializeField] private TextMeshProUGUI errorText;  // Нижняя строка (information 2)

    private Vector3 startSpindlePos;
    private bool isSpindleInitialized = false;
    private Coroutine workCoroutine;

    private void Awake()
    {
        // Если Cylinder.001 не перетащили в инспектор вручную — находим его автоматически
        if (spindleZ == null)
        {
            GameObject cylObj = GameObject.Find("Cylinder.001");
            if (cylObj != null) spindleZ = cylObj.transform;
        }

        // Запоминаем начальную позицию шпинделя
        if (spindleZ != null)
        {
            startSpindlePos = spindleZ.localPosition;
            isSpindleInitialized = true;
        }
        else
        {
            Debug.LogError("Объект Cylinder.001 не найден на сцене!");
        }

        // Автоматическая привязка кнопок Start и Stop
        Transform startTr = transform.Find("Start");
        if (startTr != null && startTr.TryGetComponent<Button>(out Button startBtn))
        {
            startBtn.onClick.AddListener(OnStartButtonClick);
        }

        Transform stopTr = transform.Find("Stop");
        if (stopTr != null && stopTr.TryGetComponent<Button>(out Button stopBtn))
        {
            stopBtn.onClick.AddListener(OnStopButtonClick);
        }
    }

    // Вызывается при нажатии на кнопку Start
    public void OnStartButtonClick()
    {
        if (stateText != null) stateText.text = "Состояние: Запущено";

        if (workCoroutine == null)
        {
            workCoroutine = StartCoroutine(ServerWorkLoop());
        }
    }

    // Вызывается при нажатии на кнопку Stop
    public void OnStopButtonClick()
    {
        if (stateText != null) stateText.text = "Состояние: Остановлено";

        // Останавливаем опрос сервера и движение Cylinder.001
        if (workCoroutine != null)
        {
            StopCoroutine(workCoroutine);
            workCoroutine = null;
        }
    }

    private IEnumerator ServerWorkLoop()
    {
        while (true)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(serverUrl))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (coordsText != null) coordsText.text = "X: - | Y: - | Z: -";
                    if (errorText != null)
                    {
                        errorText.text = $"Ошибка сети: {request.error}";
                        errorText.color = Color.red;
                    }
                }
                else
                {
                    try
                    {
                        JObject data = JObject.Parse(request.downloadHandler.text);

                        // 1. Считываем координаты X, Y, Z с сервера (в мм)
                        float x = GetCoordinate(data, "X");
                        float y = GetCoordinate(data, "Y");
                        float z = GetCoordinate(data, "Z");

                        // 2. Выводим координаты во вторую строку текста
                        if (coordsText != null)
                        {
                            coordsText.text = $"X: {x:F2} | Y: {y:F2} | Z: {z:F2}";
                        }

                        // 3. Двигаем шпиндель Cylinder.001 вертикально по координате Z
                        if (isSpindleInitialized && spindleZ != null)
                        {
                            spindleZ.localPosition = startSpindlePos + Vector3.up * (z * zScale);
                        }

                        // 4. Выводим статус отсутствия ошибок в третью строку
                        if (errorText != null)
                        {
                            errorText.text = "Ошибок нет";
                            errorText.color = Color.green;
                        }
                    }
                    catch (Exception e)
                    {
                        if (errorText != null)
                        {
                            errorText.text = $"Ошибка данных: {e.Message}";
                            errorText.color = Color.red;
                        }
                    }
                }
            }

            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, updateInterval));
        }
    }

    private float GetCoordinate(JObject data, string axis)
    {
        foreach (JToken section in data["data"])
        {
            if ((string)section[0] != "Оси Канала " + channel)
                continue;

            foreach (JToken row in section[1])
            {
                if ((string)row[0] == "Перемещение по оси " + axis)
                {
                    string text = ((string)row[1]).Replace("мм", "").Trim();
                    text = text.Replace(',', '.');
                    float value = float.Parse(text, CultureInfo.InvariantCulture);

                    if (float.IsNaN(value) || float.IsInfinity(value))
                        throw new FormatException("Некорректная координата " + axis);

                    return value;
                }
            }
        }
        throw new FormatException("Не найдена ось " + axis);
    }
}