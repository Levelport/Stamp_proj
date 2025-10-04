using UnityEngine;

[CreateAssetMenu(fileName = "NewPersonData", menuName = "Game/PersonData")]
public class PersonData : ScriptableObject
{
    public Sprite baseSprite;           // 人物のスプライト画像
    public int[] relatedDocumentIndices; // 関連するドキュメントインデックス
    public float angerTime = 10f;       // 怒りMAXまでの秒数（デフォルト10秒）
}
