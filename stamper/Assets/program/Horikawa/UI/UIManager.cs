using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI全体を統括するクラス。
/// DocumentManager, StampOperatorController, PersonManager と連携して情報表示を行う。
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("人物進行関連")]
    [SerializeField] private TextMeshProUGUI personCountText;    // 例: "人物 1 / 5"
    [SerializeField] private Slider personProgressBar;           // 残り人数の進捗バー（任意）

    [Header("セリフ関連")]
    [SerializeField] private TextMeshProUGUI dialogueText;       // セリフ（登場／退場）

    [Header("ハンコ関連")]
    [SerializeField] private TextMeshProUGUI currentStampText;   // 現在のハンコ（丸印・角印）
    [SerializeField] private TextMeshProUGUI operationModeText;  // 現在の操作モード（回転・移動・決定・待機）
    [SerializeField] private TextMeshProUGUI requiredStampsText; // 書類の必要ハンコ数

    [Header("怒りゲージ（任意）")]
    [SerializeField] private Slider angerBar;                    // PersonManagerで更新する（オプション）

    // 内部情報
    private int totalPersons = 0;
    private int currentPersonIndex = 0;

    private string currentStamp = "丸印";
    private string currentMode = "待機";
    private int currentRemaining = 0;

    // ===============================================================
    // 初期化処理
    // ===============================================================
    public void Initialize(int personCount)
    {
        totalPersons = personCount;
        currentPersonIndex = 0;

        UpdatePersonCountUI();
    }

    // ===============================================================
    // 人物進行UI
    // ===============================================================
    public void UpdatePersonInfo(int index)
    {
        currentPersonIndex = index;
        UpdatePersonCountUI();
    }

    public void NextPerson()
    {
        currentPersonIndex++;
        UpdatePersonCountUI();
    }

    private void UpdatePersonCountUI()
    {
        if (personCountText != null)
            personCountText.text = $"人物 {currentPersonIndex + 1} / {totalPersons}";

        if (personProgressBar != null)
            personProgressBar.value = (float)(currentPersonIndex + 1) / totalPersons;
    }

    // ===============================================================
    // セリフ表示
    // ===============================================================
    public void ShowDialogue(string message)
    {
        if (dialogueText != null)
            dialogueText.text = message;
    }

    public void ClearDialogue()
    {
        if (dialogueText != null)
            dialogueText.text = "";
    }

    // ===============================================================
    // ハンコ関連UI
    // ===============================================================
    public void UpdateCurrentStamp(StampType type)
    {
        string jp = (type == StampType.Circle) ? "丸印" : "角印";
        currentStamp = jp;

        if (currentStampText != null)
            currentStampText.text = $"現在のハンコ：{jp}";
    }

    public void UpdateOperationMode(string mode)
    {
        currentMode = mode;
        if (operationModeText != null)
            operationModeText.text = $"操作モード：{mode}";
    }

    public void UpdateRequiredStamps(int remaining)
    {
        currentRemaining = Mathf.Max(remaining, 0);
        if (requiredStampsText != null)
            requiredStampsText.text = $"残りハンコ数：{currentRemaining}";
    }

    // ===============================================================
    // 怒りゲージ更新（PersonManager から呼ばれる）
    // ===============================================================
    public void UpdateAngerBar(float normalizedValue)
    {
        if (angerBar != null)
            angerBar.value = Mathf.Clamp01(normalizedValue);
    }

    // ===============================================================
    // 総合UIリフレッシュ（デバッグ・再描画用）
    // ===============================================================
    public void RefreshAll()
    {
        UpdatePersonCountUI();

        if (dialogueText != null)
            dialogueText.text = dialogueText.text;

        if (currentStampText != null)
            currentStampText.text = $"現在のハンコ：{currentStamp}";

        if (operationModeText != null)
            operationModeText.text = $"操作モード：{currentMode}";

        if (requiredStampsText != null)
            requiredStampsText.text = $"残りハンコ数：{currentRemaining}";
    }
}
