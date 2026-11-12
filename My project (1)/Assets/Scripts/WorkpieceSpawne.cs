using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Valve.VR.InteractionSystem;

public class WorkpieceSpawner : MonoBehaviour
{
    [Header("Настройки заготовки")]
    public GameObject workpiecePrefab;
    public Transform spawnPoint;
    public Vector3 spawnOffset = new Vector3(0f, 0f, -0.5f);
    public float mass = 1.5f;

    [Header("Настройки текста (UI)")]
    public GameObject statusTextObject;
    public string spawnMessage = "Заготовка создана";

    private int spawnCount = 0;

    public void SpawnWorkpiece()
    {
        Vector3 position;
        if (spawnPoint != null)
        {
            position = spawnPoint.position;
        }
        else if (Camera.main != null)
        {
            Vector3 dirToPlayer = (Camera.main.transform.position - transform.position).normalized;
            dirToPlayer.y = 0f;
            position = transform.position + dirToPlayer * 0.5f + Vector3.up * 0.1f;
        }
        else
        {
            position = transform.position + transform.TransformDirection(spawnOffset);
        }

        GameObject item;
        if (workpiecePrefab != null)
        {
            // Временно выключаем шаблон перед копированием, чтобы Awake() не сработал раньше времени
            bool wasActive = workpiecePrefab.activeSelf;
            workpiecePrefab.SetActive(false);

            item = Instantiate(workpiecePrefab, position, Quaternion.identity);

            workpiecePrefab.SetActive(wasActive);
        }
        else
        {
            item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.SetActive(false);
            item.transform.position = position;
            item.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        }

        spawnCount++;
        item.name = "Workpiece_" + spawnCount;

        // Настраиваем все компоненты, пока объект выключен
        SetupPhysicsAndVR(item, position);

        // Включаем объект: теперь Awake() у Throwable точно найдет VelocityEstimator
        item.SetActive(true);

        UpdateStatusText($"{spawnMessage} ({spawnCount})");
    }

    private void SetupPhysicsAndVR(GameObject obj, Vector3 safeSpawnPos)
    {
        // Если на префабе уже был "сломанный" Throwable без VelocityEstimator — удаляем его и создаем чисто
        Throwable existingThrowable = obj.GetComponent<Throwable>();
        if (existingThrowable != null)
        {
            DestroyImmediate(existingThrowable);
        }

        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            obj.AddComponent<BoxCollider>();
        }
        else if (col is MeshCollider meshCol)
        {
            meshCol.convex = true;
        }

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.mass = mass;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (obj.GetComponent<Interactable>() == null)
        {
            obj.AddComponent<Interactable>();
        }

        // 1. Сначала обязательно VelocityEstimator
        if (obj.GetComponent<VelocityEstimator>() == null)
        {
            obj.AddComponent<VelocityEstimator>();
        }

        // 2. Затем Throwable
        Throwable throwable = obj.AddComponent<Throwable>();
        throwable.attachmentFlags = Hand.AttachmentFlags.VelocityMovement 
                                  | Hand.AttachmentFlags.TurnOffGravity 
                                  | Hand.AttachmentFlags.DetachFromOtherHand;

        WorkpieceSafetyBounds safety = obj.GetComponent<WorkpieceSafetyBounds>();
        if (safety == null)
        {
            safety = obj.AddComponent<WorkpieceSafetyBounds>();
        }
        safety.Initialize(safeSpawnPos, rb);
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

public class WorkpieceSafetyBounds : MonoBehaviour
{
    private Vector3 safePosition;
    private Rigidbody rb;
    private float maxHoldSpeed = 4f;
    private float maxDistanceFromSpawn = 6f;

    public void Initialize(Vector3 spawnPos, Rigidbody body)
    {
        safePosition = spawnPos;
        rb = body;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (rb.isKinematic)
        {
            rb.isKinematic = false;
        }

        if (rb.linearVelocity.magnitude > maxHoldSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxHoldSpeed;
        }

        if (transform.position.y < -0.5f || Vector3.Distance(transform.position, safePosition) > maxDistanceFromSpawn)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = safePosition;
        }
    }
}