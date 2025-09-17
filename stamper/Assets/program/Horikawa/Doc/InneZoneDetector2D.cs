using UnityEngine;

public class InnerZoneDetector2D : MonoBehaviour
{
    private float? bestAngle = null;

    public bool RegisterStamp(float angleDiff)
    {
        if (bestAngle == null || angleDiff < bestAngle.Value)
        {
            bestAngle = angleDiff;
            return true; // 受理された
        }

        return false; // 無視された（より良いのが既にある）
    }

    public float? GetBestAngleDiff()
    {
        return bestAngle;
    }

    public bool HasStamp()
    {
        return bestAngle != null;
    }
}
