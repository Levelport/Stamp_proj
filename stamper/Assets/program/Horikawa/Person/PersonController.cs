using UnityEngine;

public class PersonController : MonoBehaviour
{
    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer personRenderer;        // 通常の人物スプライト
    [SerializeField] private SpriteRenderer angerOverlayRenderer;  // 赤くなるオーバーレイ

    private float angerTime = 10f;  // 怒りMAXまでの時間（秒）
    private float timer = 0f;
    private bool isActive = false;

    private System.Action onAngryCallback;

    public void Init(PersonData data, System.Action onAngry)
    {
        // スプライトセット（同じ画像を2つに使う）
        personRenderer.sprite = data.baseSprite;
        angerOverlayRenderer.sprite = data.baseSprite;

        angerTime = data.angerTime;
        timer = 0f;
        isActive = true;
        onAngryCallback = onAngry;

        // 赤いオーバーレイ初期設定（透明度＋スケールY=0で下から消した状態）
        angerOverlayRenderer.color = new Color(1f, 0f, 0f, 0.5f);
        angerOverlayRenderer.transform.localScale = new Vector3(1f, 0f, 1f);
        angerOverlayRenderer.transform.localPosition = new Vector3(0f, -0.5f, 0f);
    }

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;
        float ratio = Mathf.Clamp01(timer / angerTime);

        // 赤オーバーレイを下から伸ばす演出
        angerOverlayRenderer.transform.localScale = new Vector3(1f, ratio, 1f);
        angerOverlayRenderer.transform.localPosition = new Vector3(0f, -(1f - ratio) / 2f, 0f);

        if (ratio >= 1f)
        {
            isActive = false;
            onAngryCallback?.Invoke();
        }
    }

    public void Resolve()
    {
        isActive = false;
        Destroy(gameObject, 0.5f); // 0.5秒後に消す（演出余地）
    }
}
