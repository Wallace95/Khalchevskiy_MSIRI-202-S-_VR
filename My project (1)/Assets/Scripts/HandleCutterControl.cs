using UnityEngine;
using Valve.VR.InteractionSystem;

public class HandleCutterControl : MonoBehaviour
{
    [Header("Детали ручки и фрезы")]
    [Tooltip("Перетащите сюда объект фрезы/шпинделя (например, Cylinder.001)")]
    public Transform cutter;

    [Tooltip("Перетащите сюда все 4 части ручки: Cylinder.002, 003, 005, 006")]
    public Transform[] handleParts;

    [Header("Настройки вращения и хода")]
    [Tooltip("Локальная ось вращения втулки (по скриншоту синяя стрелка — ось Z: 0, 0, 1)")]
    public Vector3 rotationAxis = new Vector3(0, 0, 1);

    [Tooltip("Радиус захвата ручки мышкой (в метрах)")]
    public float grabRadius = 0.25f;

    [Tooltip("На сколько метров сдвигается фреза за 1 полный оборот (360 градусов)")]
    public float cutterMovePerTurn = 0.08f;

    [Tooltip("Максимальное опускание вниз от старта (м)")]
    public float maxDownDistance = 0.25f;

    [Tooltip("Максимальный подъем вверх от старта (м)")]
    public float maxUpDistance = 0.15f;

    [Tooltip("Включите, если нужно поменять местами подъем и опускание")]
    public bool invertCutterDirection = false;

    private Vector3 cutterStartPos;
    private float currentCutterOffset = 0f;
    private bool isDraggingMouse = false;
    private float prevMouseAngle = 0f;
    private Camera activeCam;

    void Start()
    {
        if (cutter != null)
        {
            cutterStartPos = cutter.position;
        }

        // Гарантируем наличие коллайдеров на деталях ручки
        if (handleParts != null && handleParts.Length > 0)
        {
            foreach (Transform part in handleParts)
            {
                if (part == null) continue;
                if (part.GetComponent<Collider>() == null)
                {
                    MeshCollider mc = part.gameObject.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
            }
        }
        else if (GetComponent<Collider>() == null)
        {
            MeshCollider mc = gameObject.AddComponent<MeshCollider>();
            mc.convex = true;
        }
    }

    void Update()
    {
        Camera cam = GetActiveCamera();
        if (cam == null) return;

        // 1. Нажатие левой кнопки мыши — проверяем, кликнул ли игрок по ручке
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverHandle(cam))
            {
                isDraggingMouse = true;
                prevMouseAngle = GetMouseAngleOnScreen(cam);
            }
        }

        // 2. Удержание кнопки — считаем круговое движение курсора вокруг центра ручки
        if (isDraggingMouse && Input.GetMouseButton(0))
        {
            float currentAngle = GetMouseAngleOnScreen(cam);
            // При движении по часовой стрелке deltaClockwise будет положительным (> 0),
            // против часовой — отрицательным (< 0)
            float deltaClockwise = Mathf.DeltaAngle(currentAngle, prevMouseAngle);
            prevMouseAngle = currentAngle;

            ApplyRotationAndMove(deltaClockwise);
        }

        // 3. Отпускание кнопки мыши
        if (Input.GetMouseButtonUp(0))
        {
            isDraggingMouse = false;
        }
    }

    private bool IsMouseOverHandle(Camera cam)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Пробиваем лучом сквозь все коллайдеры (даже если корпус станка перекрывает ручку)
        RaycastHit[] hits = Physics.RaycastAll(ray, 10f);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform) return true;

            if (handleParts != null)
            {
                foreach (Transform part in handleParts)
                {
                    if (part != null && hit.transform == part)
                        return true;
                }
            }
        }

        // Дополнительная проверка по радиусу: если кликнули прямо по ручке на экране
        Vector3 pivot = GetPivotPoint();
        float distToRay = Vector3.Cross(ray.direction, pivot - ray.origin).magnitude;
        float distToPlayer = Vector3.Distance(cam.transform.position, pivot);

        return distToPlayer < 5f && distToRay <= grabRadius;
    }

    private Camera GetActiveCamera()
    {
        if (activeCam != null && activeCam.gameObject.activeInHierarchy && activeCam.enabled)
            return activeCam;

        if (Camera.main != null && Camera.main.gameObject.activeInHierarchy)
        {
            activeCam = Camera.main;
            return activeCam;
        }

        // Ищем любую активную камеру (например, FallbackObjects внутри Player)
        foreach (Camera c in Camera.allCameras)
        {
            if (c.gameObject.activeInHierarchy && c.enabled)
            {
                activeCam = c;
                return activeCam;
            }
        }
        return null;
    }

    public Vector3 GetPivotPoint()
    {
        Renderer rend = GetComponent<Renderer>();
        return rend != null ? rend.bounds.center : transform.position;
    }

    private float GetMouseAngleOnScreen(Camera cam)
    {
        Vector3 screenPivot = cam.WorldToScreenPoint(GetPivotPoint());
        Vector2 dir = (Vector2)Input.mousePosition - new Vector2(screenPivot.x, screenPivot.y);
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    private void ApplyRotationAndMove(float deltaClockwise)
    {
        if (Mathf.Abs(deltaClockwise) < 0.01f) return;

        // По умолчанию: по часовой (deltaClockwise > 0) -> фреза поднимается (+),
        // против часовой (deltaClockwise < 0) -> фреза опускается (-)
        float directionSign = invertCutterDirection ? -1f : 1f;
        float deltaMove = (deltaClockwise / 360f) * cutterMovePerTurn * directionSign;

        float newOffset = Mathf.Clamp(currentCutterOffset + deltaMove, -maxDownDistance, maxUpDistance);
        float actualMove = newOffset - currentCutterOffset;

        if (Mathf.Abs(actualMove) < 0.00001f) return;

        currentCutterOffset = newOffset;

        // 1. Двигаем фрезу вверх или вниз
        if (cutter != null)
        {
            cutter.position = cutterStartPos + Vector3.up * currentCutterOffset;
        }

        // 2. Вращаем детали ручки вокруг центра втулки
        Vector3 pivot = GetPivotPoint();
        Vector3 worldAxis = transform.TransformDirection(rotationAxis.normalized);

        if (handleParts != null && handleParts.Length > 0)
        {
            foreach (Transform part in handleParts)
            {
                if (part != null)
                {
                    part.RotateAround(pivot, worldAxis, -deltaClockwise * directionSign);
                }
            }
        }
        else
        {
            transform.RotateAround(pivot, worldAxis, -deltaClockwise * directionSign);
        }
    }
}