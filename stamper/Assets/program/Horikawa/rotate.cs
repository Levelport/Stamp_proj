using UnityEngine;
using UnityEngine.InputSystem;  // 新Input System

public class Draggable2DObjectController : MonoBehaviour
{
    [SerializeField] private Transform targetObject;

    private Vector3 originalScale;
    private bool isDragging = false;
    private float rotationZ = 0f;

    private int longPressCount = 0;

    private float pressTime = 0f;
    private float longPressDuration = 0.5f;
    private bool isPressing = false;

    private Vector2 previousInputPosition;
    private Camera mainCamera;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("ターゲットオブジェクトが設定されていません！");
            enabled = false;
            return;
        }
        originalScale = targetObject.localScale;
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector2 inputPosition = Vector2.zero;
        bool inputStarted = false;
        bool inputEnded = false;
        bool inputHeld = false;

        // --- 入力共通化：マウス or タッチ ---
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

        // --- 長押し検出 ---
        if (inputStarted)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
            worldPos.z = 0f;

            Collider2D col = Physics2D.OverlapPoint(worldPos);
            if (col != null && col.transform == targetObject)
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
            targetObject.localScale = originalScale;

            longPressCount++;

            if (longPressCount >= 3)
            {
                longPressCount = 0;
            }
        }

        if (isPressing && !isDragging && inputHeld)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= longPressDuration)
            {
                isDragging = true;
            }
        }

        if (isDragging && inputHeld)
        {
            Vector2 delta = inputPosition - previousInputPosition;

            switch (longPressCount)
            {
                case 0: // 回転モード
                    float rotationSpeed = 0.2f;
                    rotationZ -= delta.x * rotationSpeed;
                    targetObject.rotation = Quaternion.Euler(0f, 0f, rotationZ);
                    targetObject.localScale = originalScale * 1.3f;
                    break;

                case 1: // 移動モード
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
                    worldPos.z = 0f;
                    targetObject.position = worldPos;

                    // 拡大表示中
                    targetObject.localScale = originalScale * 1.3f;
                    break;

                case 2: // 何もしない
                    // Do nothing
                    break;
            }

            previousInputPosition = inputPosition;
        }
    }
}
