using UnityEngine;

public class InnerZoneDetector2D : MonoBehaviour
{
    [Header("スタンプ条件")]
    [SerializeField] private StampType requiredType; // このゾーンに必要なスタンプの種類
    [SerializeField] private float maxAcceptableAngle = 15f; // 許容される最大角度差（度）
    [SerializeField] private int maxStampCount = 2; // ゾーンに押せる最大回数

    private float? bestAngle = null;
    private bool isCorrectStamp = false;
    private bool hasIncorrectStamp = false;
    private int currentStampCount = 0;

    /// <summary>
    /// スタンプ登録（角度・種類をチェック）
    /// </summary>
    public int RegisterStamp(float angleDiff, StampType stampType)
    {
        // 押印回数制限チェック
        if (currentStampCount >= maxStampCount)
        {
            Debug.LogWarning($"⚠ 押印回数上限 ({maxStampCount}) 超過 → -20点");
            return -20;
        }

        currentStampCount++;

        // 種類が違う場合は減点
        if (stampType != requiredType)
        {
            hasIncorrectStamp = true;
            Debug.LogWarning($"❌ 間違ったスタンプ種類（期待：{requiredType}、実際：{stampType}）→ -50点");
            return -50;
        }

        isCorrectStamp = true;

        // 角度の最も正確な値を記録
        if (bestAngle == null || angleDiff < bestAngle.Value)
        {
            bestAngle = angleDiff;
            Debug.Log($"✅ 正しいスタンプ登録（角度差：{angleDiff:F2}°）");
        }

        // スコア算出
        return GetScore();
    }

    public int GetScore()
    {
        if (hasIncorrectStamp)
            return -50;

        if (!isCorrectStamp || bestAngle == null)
            return 0;

        float diff = Mathf.Clamp(bestAngle.Value, 0f, maxAcceptableAngle);
        float scoreRatio = 1f - (diff / maxAcceptableAngle);
        int baseScore = Mathf.RoundToInt(scoreRatio * 100);
        return Mathf.Clamp(baseScore, 0, 100);
    }

    public bool IsCorrectlyStamped() => isCorrectStamp;

    public void ResetStamp()
    {
        bestAngle = null;
        isCorrectStamp = false;
        hasIncorrectStamp = false;
        currentStampCount = 0;
    }

    public bool HasAnyStamp() => isCorrectStamp || hasIncorrectStamp;

    public StampType GetRequiredType() => requiredType;
}
