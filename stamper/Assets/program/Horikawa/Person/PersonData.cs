using UnityEngine;

[CreateAssetMenu(fileName = "NewPersonData", menuName = "Game/PersonData")]
public class PersonData : ScriptableObject
{
    public Sprite baseSprite;
    public int[] relatedDocumentIndices;
    public float angerTime = 10f;

    [Header("セリフ")]
    public string enterLine;
    public string exitLine;

    [Header("スタンプ構成 (Circle:1|Square:2)")]
    public string stampPattern;

    [Header("書類角度設定")]
    public string docAngleMode; // "Fixed" or "Random"
    public float docAngleMin;
    public float docAngleMax;
}
