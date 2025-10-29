using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 各書類の押印判定ゾーン。正しいハンコの種類・角度でスコアを算出。
/// DocumentManager と連携して1枚ごとの評価を返す。
/// </summary>
public class InnerZoneDetector2D : MonoBehaviour
{
    [Header("必要なスタンプタイプ")]
    public StampType requiredType = StampType.Circle;

    [Header("押印回数制限")]
    [SerializeField] private int maxStampCount = 1;

    private int currentStampCount = 0;
    private DocumentManager manager;
    private List<StampType> remainingStamps;
    private float accumulatedScore = 0f;

    private const float ANGLE_TOLERANCE = 30f; // ±30度で減点開始

    /// <summary>
    /// DocumentManager から呼ばれて設定される
    /// </summary>
    public void SetManager(DocumentManager mgr, List<StampType> required)
    {
        manager = mgr;
        remainingStamps = new List<StampType>(required);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("stamp") && currentStampCount < maxStampCount)
        {
            // スタンプの種類を推定（名前ベース）
            StampType type = collision.name.ToLower().Contains("round") ? StampType.Circle : StampType.Square;

            // 角度判定
            float diffAngle = Mathf.Abs(Mathf.DeltaAngle(collision.transform.rotation.eulerAngles.z, transform.rotation.eulerAngles.z));

            // 角度スコア（誤差が小さいほど高得点）
            float angleScore = Mathf.Clamp01(1f - (diffAngle / ANGLE_TOLERANCE)) * 100f;

            // 種類補正（種類が違えば減点）
            if (type != requiredType)
                angleScore *= 0.7f;

            accumulatedScore += angleScore;
            currentStampCount++;

            // このゾーンの必要スタンプ削除
            if (remainingStamps.Contains(type))
                remainingStamps.Remove(type);

            Debug.Log($"✅ 押印成功 [{requiredType}] → スコア {angleScore:F1}");

            // 残り必要スタンプがゼロなら書類完了
            if (remainingStamps.Count == 0)
            {
                float finalScore = accumulatedScore / currentStampCount;
                Debug.Log($"🧾 書類完了 平均スコア:{finalScore:F1}");
                manager.OnDocumentCompleted(finalScore);
            }
        }
        else if (collision.CompareTag("stamp"))
        {
            // 押印制限超過 → 減点
            Debug.Log("⚠️ 押印回数超過による減点");
            accumulatedScore -= 10f;
        }
    }
}
