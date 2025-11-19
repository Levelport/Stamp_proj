using UnityEngine;

[System.Serializable]
public class PersonData
{
    public string name;
    public string baseSpriteName;

    public int[] relatedDocumentIndices;  // 0|1|2 のような形式

    public float angerTime;

    public string enterLine;
    public string exitLine;

    // TR:Square&BR:Circle|CC:Circle のような形式
    public string stampPattern;

    public string docAngleMode;
    public float docAngleMin;
    public float docAngleMax;
}