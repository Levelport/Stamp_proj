using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PersonController : MonoBehaviour
{
    [Header("見た目")]
    [SerializeField] private SpriteRenderer bodyRenderer;

    [Header("怒りメーター")]
    [SerializeField] private Image angerBar;  // 下から赤くする Image
    private float angerTime;
    private float angerTimer = 0f;
    private bool angerActive = false;

    private PersonData myData;

    public void Setup(PersonData data)
    {
        myData = data;
        angerTime = data.angerTime;

        // Sprite 読み込み
        Sprite face = Resources.Load<Sprite>(data.baseSpriteName);
        if (face != null)
            bodyRenderer.sprite = face;

        ResetAnger();
        StartAnger();
    }

    // --------------------------------------------------------
    // 怒りメーター制御
    // --------------------------------------------------------

    public void StartAnger()
    {
        angerTimer = 0f;
        angerActive = true;
        UpdateBar();
    }

    public void StopAnger()
    {
        angerActive = false;
    }

    private void Update()
    {
        if (!angerActive) return;

        angerTimer += Time.deltaTime;

        UpdateBar();

        if (angerTimer >= angerTime)
        {
            OnAngerMax();
        }
    }

    private void UpdateBar()
    {
        if (angerBar == null) return;

        float t = angerTimer / angerTime;
        t = Mathf.Clamp01(t);

        angerBar.fillAmount = t;     // 下から赤く埋まる演出
    }

    private void ResetAnger()
    {
        angerTimer = 0f;
        angerActive = false;
        UpdateBar();
    }

    // 怒り最大時（強制退出などに使える）
    private void OnAngerMax()
    {
        angerActive = false;
        Debug.Log("怒りMAX! 強制退出などの処理！");
        // 今後仕様次第で追加
    }

    // --------------------------------------------------------
    // 書類完了時に DocumentManager から呼ばれる
    // --------------------------------------------------------
    public void OnDocumentCompleted()
    {
        // 書類ごとに怒りをリセットする
        ResetAnger();
        StartAnger();
    }

    // --------------------------------------------------------
    // UI からセリフ呼び出し用
    // --------------------------------------------------------
    public string GetEnterLine() => myData.enterLine;
    public string GetExitLine() => myData.exitLine;
}