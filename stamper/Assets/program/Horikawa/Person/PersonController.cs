using UnityEngine;

public class PersonController : MonoBehaviour
{
    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer personRenderer;        // 通常の人物スプライト
    [SerializeField] private SpriteRenderer angerOverlayRenderer;  // 赤くなるオーバーレイ

    private float angerTime = 10f;     // 怒りがMAXになるまでの秒数
    private float timer = 0f;
    private bool isActive = false;

    private System.Action onAngryCallback;

    public void Init(PersonData data, System.Action onAngry)
    {
        // スプライト画像をセット（同じ画像を両方に使う）
        personRenderer.sprite = data.baseSprite;
        angerOverlayRenderer.sprite = data.baseSprite;

        angerTime = data.angerTime;
        timer = 0f;
        isActive = true;
        onAngryCallback = onAngry;

        // 赤いオーバーレイの初期設定
        angerOverlayRenderer.color = new Color(1f, 0f, 0f, 0.5f); // 赤＋透明度
        angerOverlayRenderer.transform.localScale = new Vector3(1f, 0f, 1f); // 最初は表示しない
        angerOverlayRenderer.transform.localPosition = new Vector3(0f, -0.5f, 0f); // 下からスタート
    }

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        float ratio = Mathf.Clamp01(timer / angerTime); // 0〜1に制限

        // スケールで赤い部分を下から伸ばす
        angerOverlayRenderer.transform.localScale = new Vector3(1f, ratio, 1f);
        angerOverlayRenderer.transform.localPosition = new Vector3(0f, -(1 - ratio) / 2f, 0f);

        // 怒りMAXになったらコールバック
        if (ratio >= 1f)
        {
            isActive = false;
            onAngryCallback?.Invoke();
        }
    }

    // 書類完了時に呼び出して人を消す処理
    public void Resolve()
    {
        isActive = false;
        Destroy(gameObject, 0.5f); // 0.5秒後に消える（演出時間確保）
    }
}
