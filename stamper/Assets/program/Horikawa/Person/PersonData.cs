using UnityEngine;

[CreateAssetMenu(fileName = "NewPerson", menuName = "Game/PersonData")]
public class PersonData : ScriptableObject
{
    public Sprite baseSprite; // 人の見た目
    public int[] relatedDocumentIndices;
    public float angerTime = 10f;
}
