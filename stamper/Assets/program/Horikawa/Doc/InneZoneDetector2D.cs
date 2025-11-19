using UnityEngine;

public class InnerZoneDetector2D : MonoBehaviour
{
    [Header("ゾーンID (TL, TC, TR, CL, CC, CR, BL, BC, BR)")]
    public string zoneID;

    [Header("必要なスタンプ種別（DocumentManagerで設定される）")]
    public StampType requiredType;

    [HideInInspector]
    public bool isStamped = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("stamp"))
        {
            isStamped = true;
        }
    }
}