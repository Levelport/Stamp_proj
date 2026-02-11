// rotate.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class StampOperatorController : MonoBehaviour
{
    [Header("操作する印鑑オブジェクト")]
    [SerializeField] private Transform stampCircle;   // 丸い印鑑（操作物）
    [SerializeField] private Transform stampSquare;   // 四角い印鑑（操作物）

    [Header("押されるスタンプPrefab")]
    [SerializeField] private GameObject stampCirclePrefab;
    [SerializeField] private GameObject stampSquarePrefab;

    [Header("印鑑の待機位置")]
    [SerializeField] private Transform posCircle;
    [SerializeField] private Transform posSquare;

    [Header("参照")]
    [SerializeField] private Camera cam;
    [SerializeField] private DocumentManager doc;   // ← Inspectorで割り当て
    [SerializeField] private UIManager ui;          // 任意（モード表示用）

    [Header("長押し判定時間(秒)")]
    [SerializeField] private float longPressThreshold = 0.1f;

    [Header("押印後に戻るまでの待機時間")]
    [SerializeField] private float returnDelay = 0.3f;

    [SerializeField] private float rotateSensitivity = 1.5f;

    [Header("回転操作ヘルプ")]
    [SerializeField] private Transform rotateHelp;      // ヘルプのTransform
    [SerializeField] private Animator rotateHelpAnimator;
    [SerializeField] private Vector3 helpOffset = new Vector3(-1.2f, 0f, 0f); // 左側

    private Transform currentStamp = null;
    private StampType? currentType = null;

    private Vector3 baseCircleScale;
    private Vector3 baseSquareScale;

    // 長押しカウント（1回目＝回転、2回目＝移動、3回目＝押印）
    private int longPressCount = 0;

    // 押下状態管理
    private bool isPressing = false;
    private bool longPressActive = false;
    private bool pressStartedOnStamp = false;
    private Transform pressedStampOnDown = null;
    private float pressStartTime = 0f;

    [SerializeField] private SoundManager_H sound;

    private Vector2 lastPointerPos;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        baseCircleScale = stampCircle.localScale;
        baseSquareScale = stampSquare.localScale;



        UpdateStampUI();
        UpdateModeUI("なし");
    }

    void Awake()
    {
        float z=Random.Range(-180, 180);
        stampCircle.rotation=Quaternion.Euler(0, 0,z);
        z=Random.Range(-180, 180);
        stampSquare.rotation=Quaternion.Euler(0, 0,z);
        sound=SoundManager_H.Instance;
    }


    private void Update()
    {
        HandlePress();
    }

    // ========================================================
    // 入力処理（新 Input System 統一）
    // ========================================================
    private void HandlePress()
    {
        bool pressedNow = PrimaryPressed();
        bool downThisFrame = PrimaryDown();
        bool upThisFrame = PrimaryUp();
        Vector2 pointerPos = GetPointerPos();

        if (downThisFrame)
        {
            // 押し始め
            isPressing = true;
            longPressActive = false;
            pressStartTime = Time.time;
            lastPointerPos = pointerPos;

            // 押し始め位置がどのハンコのコライダー上か判定
            pressedStampOnDown = null;
            pressStartedOnStamp = false;

            Vector3 world = cam.ScreenToWorldPoint(pointerPos);
            world.z = 0f;
            RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.transform == stampCircle || hit.transform == stampSquare)
                {
                    pressedStampOnDown = hit.transform;
                    pressStartedOnStamp = true;
                }
            }
        }

        if (pressedNow && isPressing)
        {
            // 押している間
            float heldTime = Time.time - pressStartTime;

            if (!longPressActive && pressStartedOnStamp && heldTime >= longPressThreshold)
            {
                // ここで「長押し」とみなす（1回分カウント）
                longPressActive = true;
                longPressCount++;

                // 3回を超えたら1回目に戻す
                if (longPressCount > 3) longPressCount = 1;

                // まだ選択されていない or 別のハンコの場合、このタイミングで選択
                if (currentStamp == null || currentStamp != pressedStampOnDown)
                {
                    if (pressedStampOnDown == stampCircle)
                        SelectStamp(stampCircle, StampType.Circle);
                    else if (pressedStampOnDown == stampSquare)
                        SelectStamp(stampSquare, StampType.Square);
                }

                // 長押し回数に応じてモード決定
                switch (longPressCount)
                {
                    case 1:
                        UpdateModeUI("回転");
                        ShowRotateHelp(true);
                        break;
                    case 2:
                        UpdateModeUI("移動");
                        break;
                    case 3:
                        UpdateModeUI("押印中");
                        break;
                }
            }

            // 長押し中の処理（モードごと）
            if (longPressActive && currentStamp != null)
            {
                switch (longPressCount)
                {
                    case 1: // 回転
                        RotateStamp(pointerPos);
                        break;
                    case 2: // 移動
                        MoveStamp(pointerPos);
                        break;
                    case 3:
                        // 押印準備中：押している間は特に何もしない（ポーズイメージ）
                     // 3回目の長押し → 離した瞬間に押印
                    //PerformStamp();
                    // カウンタリセット
                    //longPressCount = 0;
                    //UpdateModeUI("なし");
                        break;
                }
            }

            lastPointerPos = pointerPos;
        }

        if (upThisFrame && isPressing)
        {
            // 離した瞬間
            float heldTime = Time.time - pressStartTime;

            if (!longPressActive && pressStartedOnStamp && heldTime < longPressThreshold )
            {
                // これは「普通のタップ」＝選択処理
                if (pressedStampOnDown == stampCircle)
                    SelectStamp(stampCircle, StampType.Circle);
                else if (pressedStampOnDown == stampSquare)
                    SelectStamp(stampSquare, StampType.Square);
            }
            else if (longPressActive)
            {
                // これは「長押し完了」
                if (longPressCount == 3)
                {
                    // 3回目の長押し → 離した瞬間に押印
                    PerformStamp();
                    // カウンタリセット
                    longPressCount = 0;
                    UpdateModeUI("なし");
                }
                else if(longPressCount==1)
                {
                    ShowRotateHelp(false);
                }
                // 1回目・2回目の長押しは、離してもカウンタは維持して次の長押しに備える
            }

            // 押下状態リセット
            isPressing = false;
            longPressActive = false;
            pressStartedOnStamp = false;
            pressedStampOnDown = null;
        }
    }

    // ========================================================
    // ハンコ選択
    // ========================================================
    private void SelectStamp(Transform t, StampType type)
    {
            // 現在のハンコを元の位置に戻す
    if (currentStamp != null)
    {
        if (currentType == StampType.Circle)
            currentStamp.position = posCircle.position;
        else if (currentType == StampType.Square)
            currentStamp.position = posSquare.position;

        // スケールも元に戻す
        currentStamp.localScale = (currentType == StampType.Circle) ? baseCircleScale : baseSquareScale;
    }

    // 新しいハンコを選択
    currentStamp = t;
    currentType = type;

    // 選択された印鑑だけ1.3倍
    Vector3 nowScale = currentStamp.localScale;
    currentStamp.localScale = nowScale * 1.3f;

    // 長押しカウンタはリセット
    longPressCount = 0;
    longPressActive = false;

    UpdateStampUI();
    UpdateModeUI("なし");
        /*
        currentStamp = t;
        currentType = type;

        // 元のスケールに戻す
        stampCircle.localScale = baseCircleScale;
        stampSquare.localScale = baseSquareScale;

        // 選択された印鑑だけ1.3倍
        Vector3 nowScale = currentStamp.localScale;
        currentStamp.localScale = nowScale * 1.3f;

        // ハンコを選び直したら長押しカウンタはリセット
        longPressCount = 0;
        longPressActive = false;

        UpdateStampUI();
        UpdateModeUI("なし");*/
    }

    // ========================================================
    // 回転（横移動）
    // ========================================================
    private void RotateStamp(Vector2 pointerPos)
    {
        if (currentStamp == null) return;

        float dx = pointerPos.x - lastPointerPos.x;

        // 画面幅を正規化して回転量に変換（720px幅を想定だが、Screen.widthでもOK）
        float width = Screen.width > 0 ? Screen.width : 720f;
        float ratio = dx / width;  // 画面幅分スライドで ratio=1
        float angleDelta = ratio * 360f * rotateSensitivity; // 横1画面分ドラッグで1回転

        currentStamp.Rotate(0, 0, angleDelta);
    }

    // ========================================================
    // 移動（指／マウス追従）
    // ========================================================
    private void MoveStamp(Vector2 pointerPos)
    {
        if (currentStamp == null) return;

        Vector3 world = cam.ScreenToWorldPoint(pointerPos);
        world.z = -1.0f;  // 印鑑のZ
        currentStamp.position = world;
    }

    // ========================================================
    // 押印（3回目の長押しを離した瞬間）
    // ========================================================
    private void PerformStamp()
    {
        if (currentStamp == null || currentType == null) return;
        if (doc == null)
        {
            Debug.LogWarning("StampOperatorController: DocumentManagerへの参照(doc)が設定されていません");
            return;
        }

        GameObject prefab = (currentType == StampType.Circle)
            ? stampCirclePrefab
            : stampSquarePrefab;

        Vector3 spawnPos = currentStamp.position;
        spawnPos.z = -1.0f;    // 書類と同じZに押されるスタンプを配置

        GameObject stamped = Instantiate(prefab, spawnPos, currentStamp.rotation);
        stamped.tag = "stamp";

        SoundManager_H.Instance.PlaySE("stamp",5f);

        
        // DocumentManager に押印完了を通知
        doc.OnStampPlaced(stamped);
/*
        float z=Random.Range(-180, 180);
        currentStamp.rotation=Quaternion.Euler(0, 0,z);
        // 印鑑操作物は元の位置に戻す
        if (currentType == StampType.Circle)
            currentStamp.position = posCircle.position;
        else
            currentStamp.position = posSquare.position;
*/
        StartCoroutine(ReturnStampAfterDelay());
    }

    // ========================================================
    // 新 Input System 用ヘルパ
    // ========================================================
    private bool PrimaryPressed()
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null && touch.press.isPressed)
            return true;

        return Mouse.current?.leftButton.isPressed ?? false;
    }

    private bool PrimaryDown()
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null && touch.press.wasPressedThisFrame)
            return true;

        return Mouse.current?.leftButton.wasPressedThisFrame ?? false;
    }

    private bool PrimaryUp()
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null && touch.press.wasReleasedThisFrame)
            return true;

        return Mouse.current?.leftButton.wasReleasedThisFrame ?? false;
    }

    private Vector2 GetPointerPos()
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch != null)
            return touch.position.ReadValue();

        return Mouse.current?.position.ReadValue() ?? Vector2.zero;
    }

    // ========================================================
    // UI 更新
    // ========================================================
    private void UpdateStampUI()
    {
        if (ui == null) return;
        ui.UpdateCurrentStamp(currentType);
    }

    private void UpdateModeUI(string mode)
    {
        if (ui == null) return;
        ui.UpdateOperationMode(mode);
    }

private IEnumerator ReturnStampAfterDelay()
{
    // 少し待つ
    yield return new WaitForSeconds(returnDelay);

    // 角度をランダムに
    float z = Random.Range(-180, 180);
    currentStamp.rotation = Quaternion.Euler(0, 0, z);

    // 待機位置へ戻す
    if (currentType == StampType.Circle)
        currentStamp.position = posCircle.position;
    else
        currentStamp.position = posSquare.position;
}


private void ShowRotateHelp(bool show)
{
    if (rotateHelp == null) return;

    rotateHelp.gameObject.SetActive(show);

    if (rotateHelpAnimator != null)
        rotateHelpAnimator.SetBool("isRotating", show);

    if (show && currentStamp != null)
    {
        rotateHelp.position = currentStamp.position + helpOffset;
    }
}

private void LateUpdate()
{
    if (rotateHelp != null && rotateHelp.gameObject.activeSelf && currentStamp != null)
    {
        rotateHelp.position = currentStamp.position + helpOffset;
    }
}


}
