using UnityEngine;

public class InnerZoneDetector2D : MonoBehaviour
{
    [Header("Zone ID (TL, TC, TR, CL, CC, CR, BL, BC, BR)")]
    public string zoneID;

    [Header("必要スタンプ種類")]
    public StampType requiredType;

    public bool isStamped = false;

    private Collider2D col;
    [Header("radius Score")]
    public  float inrad=0.1f;
    public  float outrad=0.6f;

    [Header("Angle Score")]
    public float perfectAngle = 3f;   // ±何度まで満点か
    public float maxAngle = 40f;      // ここで0点

    void Awake()
    {
        col = GetComponent<Collider2D>();
        isStamped = false;
    }

    /// <summary>
    /// ゾーン中心座標（Collider の中心）
    /// </summary>
    public Vector3 Center
    {
        get
        {
            if (col != null)
                return col.bounds.center;
            return transform.position;
        }
    }

/// <summary>
    /// スコア計算 ※ステータスの変更はしない
    /// </summary>
    public float CalculateScore(GameObject stampObj)
    {
        // ★ isStamped = true はここでは絶対にしない

        Vector3 stampPos = stampObj.transform.position;
        Vector3 zoneCenter = Center;

        float dist = Vector2.Distance(stampPos, zoneCenter);


        float distScore ;
        if (dist <= inrad)
        {
        distScore = 1f;
        }
        else if(dist <=outrad)
        {
        float t = (dist - inrad) / (outrad - inrad);
        distScore = Mathf.Clamp01(1f - t);
        }
        else
        {
            distScore=0;
        }

        float stampAngle = stampObj.transform.rotation.eulerAngles.z;
        float zoneAngle = transform.rotation.eulerAngles.z;
        float diffAngle = Mathf.Abs(Mathf.DeltaAngle(stampAngle, zoneAngle));
        float angleScore;

        if (diffAngle <= perfectAngle)
        {
            // 満点ゾーン
            angleScore = 1f;
        }
        else if (diffAngle >= maxAngle)
        {
            // 完全に外れ
            angleScore = 0f;
        }
        else
        {
            // perfectAngle ～ maxAngle の間を線形減衰
            float t = (diffAngle - perfectAngle) / (maxAngle - perfectAngle);
            angleScore = 1f - t;
        }
        bool match = stampObj.name.ToLower().Contains(requiredType.ToString().ToLower());
        float typeScore = match ? 1f : 0.4f;

        float finalScore = (distScore * 0.45f + angleScore * 0.45f + typeScore * 0.10f) * 100f;
        return finalScore;
    }

    /// <summary>
    /// ★ 押印完了を DocumentManager が知らせるための関数
    /// </summary>
    public void MarkStamped()
    {
        isStamped = true;
    }
}
