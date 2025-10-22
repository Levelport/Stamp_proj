using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class Draggable2DObjectController : MonoBehaviour
{
    [Header("Handlers & Origins")]
    [SerializeField] private Transform circleHandler;
    [SerializeField] private Transform squareHandler;
    [SerializeField] private Transform circleOrigin;
    [SerializeField] private Transform squareOrigin;

    [Header("Stamp Prefabs")]
    [SerializeField] private GameObject circleStampPrefab;
    [SerializeField] private GameObject squareStampPrefab;

    [Header("Visual Feedback")]
    [SerializeField] private CanvasGroup circleCanvasGroup;
    [SerializeField] private CanvasGroup squareCanvasGroup;

    [Header("System")]
    [SerializeField] private DocumentManager documentManager;

    private InnerZoneDetector2D[] innerZones;
    private Transform outerZoneTransform;
    private bool isDragging = false;
    private float rotationZ = 0f;
    private int longPressCount = 0;
    private float pressTime = 0f;
    private float longPressDuration = 0.5f;
    private bool isPressing = false;
    private Vector2 previousInputPosition;
    private Camera mainCamera;

    private List<StampType> requiredStamps = new List<StampType>();
    private int currentStampIndex = 0;
    private StampType currentStampType;

    private Vector3 circleOriginalScale;
    private Vector3 squareOriginalScale;

    public void SetStampZones(InnerZoneDetector2D[] zones, Transform outer)
    {
        innerZones = zones;
        outerZoneTransform = outer;
        currentStampIndex = 0;

        requiredStamps = documentManager.GetRequiredStamps();
        if (requiredStamps.Count > 0)
        {
            currentStampType = requiredStamps[0];
            SetActiveHandler();
        }

        ResetHandlerPositions();
    }

    void Start()
    {
        mainCamera = Camera.main;
        circleOriginalScale = circleHandler.localScale;
        squareOriginalScale = squareHandler.localScale;
        ResetHandlerPositions();
    }

    void Update()
    {
        Vector2 inputPosition = Vector2.zero;
        bool inputStarted = false;
        bool inputEnded = false;
        bool inputHeld = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        var mouse = Mouse.current;
        if (mouse != null)
        {
            inputPosition = mouse.position.ReadValue();
            inputStarted = mouse.leftButton.wasPressedThisFrame;
            inputEnded = mouse.leftButton.wasReleasedThisFrame;
            inputHeld = mouse.leftButton.isPressed;
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;
            switch (touch.phase)
            {
                case TouchPhase.Began: inputStarted = true; break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled: inputEnded = true; break;
                default: inputHeld = true; break;
            }
        }
#endif

        Transform handler = GetCurrentHandler();
        Vector3 originalScale = currentStampType == StampType.Circle ? circleOriginalScale : squareOriginalScale;

        if (inputStarted)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
            worldPos.z = 0f;
            Collider2D col = Physics2D.OverlapPoint(worldPos);
            if (col != null && col.transform == handler)
            {
                isPressing = true;
                pressTime = 0f;
                previousInputPosition = inputPosition;
            }
        }

        if (inputEnded)
        {
            isPressing = false;
            isDragging = false;
            handler.localScale = originalScale;

            longPressCount++;

            if (longPressCount >= 3)
            {
                longPressCount = 0;
                TryPlaceStamp();
                StartCoroutine(DelayedResetCurrentHandler(0.3f));
            }
        }

        if (isPressing && !isDragging && inputHeld)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= longPressDuration)
                isDragging = true;
        }

        if (isDragging && inputHeld)
        {
            Vector2 delta = inputPosition - previousInputPosition;

            switch (longPressCount)
            {
                case 0:
                    rotationZ -= delta.x * 0.2f;
                    handler.rotation = Quaternion.Euler(0f, 0f, rotationZ);
                    handler.localScale = originalScale * 1.3f;
                    break;

                case 1:
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
                    worldPos.z = -1f;
                    handler.position = worldPos;
                    handler.localScale = originalScale * 1.3f;
                    break;
            }

            previousInputPosition = inputPosition;
        }
    }

    private void TryPlaceStamp()
    {
        if (currentStampIndex >= requiredStamps.Count)
            return;

        Transform handler = GetCurrentHandler();
        Vector3 pos = handler.position;
        Quaternion rot = handler.rotation;

        float outerAngle = outerZoneTransform.eulerAngles.z;
        float stampAngle = rot.eulerAngles.z;
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(outerAngle, stampAngle));

        bool validStamp = false;
        int score = 0;

        foreach (var zone in innerZones)
        {
            Collider2D col = zone.GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(pos))
            {
                score = zone.RegisterStamp(angleDiff, currentStampType);
                documentManager.AddScore(score);

                Instantiate(GetStampPrefab(), pos, rot).tag = "stamp";

                if (score >= 70)
                {
                    Debug.Log($"✅ スタンプ成功: {currentStampType}（スコア: {score}）");
                    currentStampIndex++;
                    if (currentStampIndex >= requiredStamps.Count)
                    {
                        documentManager.OnDocumentCompleted(); // ★ 書類完了を通知
                        StartCoroutine(DelayedLoadNextDocument(1.5f));
                    }
                }
                else
                {
                    Debug.Log($"⚠ スタンプ失敗（スコア: {score}）");
                }

                validStamp = true;
                break;
            }
        }

        if (!validStamp)
        {
            Vector3 stampPos = pos;
            stampPos.z = -0.1f;
            Instantiate(GetStampPrefab(), stampPos, rot).tag = "stamp";
            Debug.Log("❌ 内部ゾーン外：スコアなし（カウント外）");
        }
    }

    private IEnumerator DelayedLoadNextDocument(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] stamps = GameObject.FindGameObjectsWithTag("stamp");
        foreach (GameObject stamp in stamps)
            Destroy(stamp);

        documentManager.LoadNextDocument();
    }

    private Transform GetCurrentHandler() =>
        currentStampType == StampType.Circle ? circleHandler : squareHandler;

    private Transform GetCurrentOrigin() =>
        currentStampType == StampType.Circle ? circleOrigin : squareOrigin;

    private GameObject GetStampPrefab() =>
        currentStampType == StampType.Circle ? circleStampPrefab : squareStampPrefab;

    private void SetActiveHandler()
    {
        bool isCircle = currentStampType == StampType.Circle;
        circleCanvasGroup.alpha = isCircle ? 1f : 0.4f;
        circleCanvasGroup.interactable = isCircle;
        squareCanvasGroup.alpha = isCircle ? 0.4f : 1f;
        squareCanvasGroup.interactable = !isCircle;
    }

    private void ResetHandlerPositions()
    {
        circleHandler.position = circleOrigin.position;
        circleHandler.rotation = circleOrigin.rotation;
        squareHandler.position = squareOrigin.position;
        squareHandler.rotation = squareOrigin.rotation;
    }

    private IEnumerator DelayedResetCurrentHandler(float delay)
    {
        yield return new WaitForSeconds(delay);

        Transform handler = GetCurrentHandler();
        Transform origin = GetCurrentOrigin();

        handler.position = origin.position;
        handler.rotation = origin.rotation;

        if (currentStampIndex < requiredStamps.Count)
        {
            currentStampType = requiredStamps[currentStampIndex];
            SetActiveHandler();
        }
    }
}
