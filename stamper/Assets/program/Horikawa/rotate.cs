using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 印鑑操作：長押し1回目で回転、2回目で移動、3回目で押印（決定）
/// 単タップで丸⇔四角のハンコ切替
/// </summary>
public class StampOperatorController : MonoBehaviour
{
    [Header("操作対象 Stamp（印鑑本体）")]
    [SerializeField] private Transform stampRound;   // 丸ハンコ操作物
    [SerializeField] private Transform stampSquare;  // 四角ハンコ操作物

    [Header("印影プレハブ")]
    [SerializeField] private GameObject roundStampPrefab;
    [SerializeField] private GameObject squareStampPrefab;

    [Header("初期位置（待機ポジション）")]
    [SerializeField] private Transform stampPos1;
    [SerializeField] private Transform stampPos2;

    [Header("設定値")]
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float holdThreshold = 0.6f;
    [SerializeField] private float zOffset = 1.0f;

    private enum StampMode { Idle, Rotate, Move, Confirm }
    private StampMode currentMode = StampMode.Idle;

    private Transform activeStamp;
    private GameObject activePrefab;
    private Camera mainCam;
    private Vector3 dragStart;
    private float currentRotation = 0f;
    private bool inputPressedLastFrame = false;
    private float holdTimer = 0f;
    private int holdCount = 0; // 1=回転, 2=移動, 3=押印

    void Start()
    {
        mainCam = Camera.main;
        SetActiveStamp(StampType.Circle);
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#elif UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#endif
    }

    // 🖱️ PC操作
    private void HandleMouseInput()
    {
        Vector2 pos = Mouse.current.position.ReadValue();
        bool pressed = Mouse.current.leftButton.isPressed;
        ProcessInput(pos, pressed);
    }

    // 🤚 モバイル操作
    private void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;
        Vector2 pos = touch.position.ReadValue();
        bool pressed = touch.press.isPressed;

        ProcessInput(pos, pressed);
    }

    // 共通入力処理
    private void ProcessInput(Vector2 screenPos, bool pressed)
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zOffset));
        worldPos.z = zOffset;

        if (pressed)
        {
            holdTimer += Time.deltaTime;

            if (!inputPressedLastFrame)
            {
                dragStart = worldPos;
            }

            // 長押し判定
            if (holdTimer >= holdThreshold && !inputPressedLastFrame)
            {
                holdCount++;
                holdTimer = 0f;

                switch (holdCount)
                {
                    case 1:
                        currentMode = StampMode.Rotate;
                        Debug.Log("🌀 回転モード開始");
                        break;
                    case 2:
                        currentMode = StampMode.Move;
                        Debug.Log("📦 移動モード開始");
                        break;
                    case 3:
                        currentMode = StampMode.Confirm;
                        Debug.Log("✅ 押印確定");
                        TryStamp();
                        ResetToOrigin();
                        holdCount = 0;
                        currentMode = StampMode.Idle;
                        break;
                }
            }

            if (currentMode == StampMode.Rotate)
            {
                Vector2 delta = screenPos - new Vector2(dragStart.x, dragStart.y);
                float rotDelta = delta.x * rotationSpeed * Time.deltaTime;
                currentRotation += rotDelta;
                activeStamp.rotation = Quaternion.Euler(0, 0, currentRotation);
            }

            if (currentMode == StampMode.Move)
            {
                activeStamp.position = Vector3.Lerp(activeStamp.position, worldPos, 0.5f);
            }
        }
        else
        {
            if (inputPressedLastFrame && holdTimer < holdThreshold)
            {
                // 単タップ → ハンコ切替
                ToggleStamp();
            }

            holdTimer = 0;
        }

        inputPressedLastFrame = pressed;
    }

    private void TryStamp()
    {
        if (activePrefab == null) return;

        Vector3 pos = activeStamp.position;
        pos.z = 0f; // 書類上（Z=0）に押す
        Instantiate(activePrefab, pos, activeStamp.rotation).tag = "stamp";
    }

    private void ResetToOrigin()
    {
        Transform targetPos = (activeStamp == stampRound) ? stampPos1 : stampPos2;
        activeStamp.position = targetPos.position;
        activeStamp.rotation = Quaternion.identity;
        currentRotation = 0f;
    }

    private void SetActiveStamp(StampType type)
    {
        if (type == StampType.Circle)
        {
            stampRound.gameObject.SetActive(true);
            stampSquare.gameObject.SetActive(false);
            activeStamp = stampRound;
            activePrefab = roundStampPrefab;
        }
        else
        {
            stampRound.gameObject.SetActive(false);
            stampSquare.gameObject.SetActive(true);
            activeStamp = stampSquare;
            activePrefab = squareStampPrefab;
        }
        ResetToOrigin();
    }

    private void ToggleStamp()
    {
        if (activeStamp == stampRound)
            SetActiveStamp(StampType.Square);
        else
            SetActiveStamp(StampType.Circle);
    }
}
