using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// ハンコ操作管理クラス
/// ・長押し1回目：回転（横スライドで回転）
/// ・長押し2回目：移動（タッチ位置に即追従）
/// ・長押し3回目：押印（離した瞬間押印）
/// </summary>
public class StampOperatorController : MonoBehaviour
{
    [Header("印影プレハブ")]
    [SerializeField] private GameObject roundStampPrefab;
    [SerializeField] private GameObject squareStampPrefab;

    [Header("ハンコ本体（柄）")]
    [SerializeField] private Transform stampRoundHandle;
    [SerializeField] private Transform stampSquareHandle;

    [Header("ハンコ初期位置")]
    [SerializeField] private Transform stampPos1;
    [SerializeField] private Transform stampPos2;

    [Header("操作パラメータ")]
    [SerializeField] private float moveSpeed = 30f;       // 移動追従速度
    [SerializeField] private float rotationSpeed = 360f;  // スライド1画面分＝360°
    [SerializeField] private float longPressThreshold = 0.4f;

    [Header("参照")]
    public DocumentManager documentManager;
    public UIManager uiManager;

    private Camera mainCam;
    private Transform activeHandle;
    private StampType currentType ;

    private bool isPointerDown = false;
    private bool canSwitchStamp = true;
    private bool isStampPressed = false;

    private float holdTimer = 0f;
    private int holdCount = 0;

    private Vector2 lastPointerPos;
    private float accumulatedRotation = 0f;
    private Vector3 roundOriginalScale;
    private Vector3 squareOriginalScale;

    private void Start()
    {
        mainCam = Camera.main;
        roundOriginalScale = stampRoundHandle.localScale;
        squareOriginalScale = stampSquareHandle.localScale;
        SetActiveHandle(StampType.Circle);
    }

    private void Update()
    {
        HandlePointer();
    }

    // ================================================================
    // 入力処理
    // ================================================================
    private void HandlePointer()
    {
        Vector2 pointerPos = GetPointerScreenPos();
        bool isPress = IsPointerPressed();

        Transform hitHandle = GetHandleUnderPointer(pointerPos);

        if (isPress)
        {
            if (!isPointerDown)
            {
                isPointerDown = true;
                holdTimer = 0f;
                lastPointerPos = pointerPos;

                // 別のハンコをタップしたら切り替え
                if (hitHandle != null && canSwitchStamp && hitHandle != activeHandle)
                {
                    if (hitHandle == stampRoundHandle)
                        SetActiveHandle(StampType.Circle);
                    else if (hitHandle == stampSquareHandle)
                        SetActiveHandle(StampType.Square);
                }
            }

            holdTimer += Time.deltaTime;

            if (holdTimer > longPressThreshold)
            {
                // 最初の長押しでモード進行
                if (holdTimer < longPressThreshold + 0.02f)
                {
                    holdCount++;
                    OnHoldStep();
                }

                if (holdCount == 1)
                    HandleRotation(pointerPos);
                else if (holdCount == 2)
                    HandleMove(pointerPos);
            }

            lastPointerPos = pointerPos;
        }
        else
        {
            if (isPointerDown)
            {
                if (isStampPressed)
                {
                    StartCoroutine(PerformStamp());
                    isStampPressed = false;
                }

                isPointerDown = false;
                holdTimer = 0f;
            }
        }
    }

    // ================================================================
    // 長押しモード進行
    // ================================================================
    private void OnHoldStep()
    {
        switch (holdCount)
        {
            case 1:
                uiManager.UpdateOperationMode("回転モード");
                Debug.Log("🌀 回転モード開始");
                break;
            case 2:
                uiManager.UpdateOperationMode("移動モード");
                Debug.Log("↔️ 移動モード開始");
                break;
            case 3:
                StartStampMode();
                break;
            default:
                holdCount = 3;
                break;
        }
    }

    private void StartStampMode()
    {
        canSwitchStamp = false;
        isStampPressed = true;
        uiManager.UpdateOperationMode("押印中...");
        Debug.Log("🧾 押印モード：長押し解除で押印確定");
    }

    // ================================================================
    // 横スライドで回転（1画面分 ≒ 360°）
    // ================================================================
    private void HandleRotation(Vector2 pointerPos)
    {
        Vector2 delta = pointerPos - lastPointerPos;

        float screenWidth = Screen.width;
        float deltaXNormalized = delta.x / screenWidth; // -1〜1 の範囲
        accumulatedRotation -= deltaXNormalized * rotationSpeed; // 横移動量で回転

        activeHandle.rotation = Quaternion.Euler(0, 0, accumulatedRotation);
    }

    // ================================================================
    // 移動操作（即追従・取り残し防止）
    // ================================================================
    private void HandleMove(Vector2 pointerPos)
    {
        Vector3 worldPos = GetPointerWorldPos();
        Vector3 targetPos = new Vector3(worldPos.x, worldPos.y, activeHandle.position.z);
        activeHandle.position = Vector3.MoveTowards(
            activeHandle.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    // ================================================================
    // 押印確定処理
    // ================================================================
    private IEnumerator PerformStamp()
    {
        uiManager.UpdateOperationMode("押印中...");
        canSwitchStamp = false;

        // 軽い沈み込み演出
        Vector3 pressPos = activeHandle.position + new Vector3(0, 0, -0.1f);
        Vector3 startPos = activeHandle.position;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            activeHandle.position = Vector3.Lerp(startPos, pressPos, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        // 印影生成
        Vector3 stampPos = activeHandle.position;
        stampPos.z = 0f;
        GameObject prefab = (currentType == StampType.Circle) ? roundStampPrefab : squareStampPrefab;
        Instantiate(prefab, stampPos, activeHandle.rotation).tag = "stamp";

        yield return new WaitForSeconds(0.3f);
        documentManager.OnStampFinished();

        // 元位置へ戻す
        yield return StartCoroutine(ReturnToStart());

        holdCount = 0;
        canSwitchStamp = true;
        uiManager.UpdateOperationMode("待機");
    }

    private IEnumerator ReturnToStart()
    {
        Vector3 target = (currentType == StampType.Circle) ? stampPos1.position : stampPos2.position;
        Vector3 start = activeHandle.position;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            activeHandle.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
    }

    // ================================================================
    // ハンコ切替
    // ================================================================
    private void SetActiveHandle(StampType type)
    {
        stampRoundHandle.localScale = roundOriginalScale;
        stampSquareHandle.localScale = squareOriginalScale;

        if (type == StampType.Circle)
        {
            activeHandle = stampRoundHandle;
            stampRoundHandle.localScale = roundOriginalScale * 1.3f;
        }
        else
        {
            activeHandle = stampSquareHandle;
            stampSquareHandle.localScale = squareOriginalScale * 1.3f;
        }

        currentType = type;
        accumulatedRotation = activeHandle.rotation.eulerAngles.z;
        uiManager.UpdateCurrentStamp(type);

        Debug.Log($"✅ ハンコ選択：{type}");
    }

    // ================================================================
    // 入力・Raycastヘルパー
    // ================================================================
    private Vector3 GetPointerWorldPos()
    {
        Vector2 screen = GetPointerScreenPos();
        Vector3 world = mainCam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
        world.z = activeHandle.position.z;
        return world;
    }

    private Vector2 GetPointerScreenPos()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        else
            return Mouse.current.position.ReadValue();
    }

    private bool IsPointerPressed()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.isPressed;
        else
            return Mouse.current.leftButton.isPressed;
    }

    private Transform GetHandleUnderPointer(Vector2 screenPos)
    {
        Ray ray = mainCam.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider == null) return null;
        if (hit.collider.transform == stampRoundHandle) return stampRoundHandle;
        if (hit.collider.transform == stampSquareHandle) return stampSquareHandle;
        return null;
    }

    public void UpdateMaxStampCount(int count, DocumentManager mgr)
    {
        documentManager = mgr;
    }
}