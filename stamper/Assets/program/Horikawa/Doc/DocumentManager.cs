using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DocumentManager : MonoBehaviour
{
    [Header("Documents")]
    [SerializeField] private GameObject[] documentPrefabs;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private UIManager uiManager;

    private GameObject currentDocument;

    // personDatas は別で読んでいるため外部でセットされる想定でも OK
    private PersonData[] personDatas;
    private int currentPersonIndex = 0;

    private PersonController currentPerson;

    private PersonData activePerson;
    private int docIndex = 0;

    private int stampsPlaced = 0;
    private int requiredStampCount = 0;

    // 外部から personDatas をセットする関数
    public void Setup(PersonData[] datas)
    {
        personDatas = datas;
    }

    public void SetCurrentPerson(PersonController person)
    {
        currentPerson = person;

    }

    public void StartProcessForPerson(PersonData person)
    {
        activePerson = person;
        docIndex = 0;
        StartCoroutine(DocumentRoutine());
    }

    public void StartProcess()
    {
        StartCoroutine(ProcessPeopleRoutine());
    }

    private IEnumerator ProcessPeopleRoutine()
    {
        foreach (var person in personDatas)
        {
            foreach (int docIndex in person.relatedDocumentIndices)
            {
                LoadDocument(person, docIndex);

                yield return new WaitUntil(() => currentDocument == null);

                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    private void LoadDocument(PersonData person, int docIndex)
    {
        if (currentDocument != null)
            Destroy(currentDocument);

        // 書類生成
        float angle =
            (person.docAngleMode == "Random")
            ? Random.Range(person.docAngleMin, person.docAngleMax)
            : person.docAngleMin;

        currentDocument = Instantiate(
            documentPrefabs[docIndex],
            spawnPoint.position,
            Quaternion.Euler(0, 0, angle)
        );

        stampsPlaced = 0;

        // ▼ TR / TL / BR のパターンを InnerZoneDetector2D に適用する
        ApplyStampPattern(person, docIndex);

        requiredStampCount = CountRequiredStamps(currentDocument);
    }

    // ---------------------------------------------------------
    // TR:Square&BR:Circle|CC:Circle → zoneID → StampType
    // ---------------------------------------------------------
    private Dictionary<string, StampType> ParseStampPattern(string pattern, int docIndex)
    {
        Dictionary<string, StampType> result = new Dictionary<string, StampType>();

        if (string.IsNullOrEmpty(pattern)) return result;

        string[] docRules = pattern.Split('|');
        if (docIndex >= docRules.Length) return result;

        string docRule = docRules[docIndex];
        string[] zoneRules = docRule.Split('&');

        foreach (string rule in zoneRules)
        {
            string[] kv = rule.Split(':');
            if (kv.Length != 2) continue;

            string zoneID = kv[0].Trim();
            string typeStr = kv[1].Trim();

            if (System.Enum.TryParse(typeStr, out StampType type))
            {
                result[zoneID] = type;
            }
        }

        return result;
    }

    // ---------------------------------------------------------
    // InnerZoneDetector2D に必要スタンプ種別をセット
    // ---------------------------------------------------------
    private void ApplyStampPattern(PersonData person, int docIndex)
    {
        var zoneMap = ParseStampPattern(person.stampPattern, docIndex);

        InnerZoneDetector2D[] zones =
            currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();

        foreach (var zone in zones)
        {
            if (zoneMap.TryGetValue(zone.zoneID, out StampType type))
            {
                zone.requiredType = type;
                Debug.Log($"InnerZone {zone.zoneID} ← {type}");
            }
            else
            {
                Debug.LogWarning($"CSVに {zone.zoneID} の設定がありません");
            }
        }
    }

    // ---------------------------------------------------------
    private int CountRequiredStamps(GameObject doc)
    {
        return doc.GetComponentsInChildren<InnerZoneDetector2D>().Length;
    }

    // ---------------------------------------------------------
    // ハンコが押された時 DocumentManager から呼ばれる想定
    // ---------------------------------------------------------
    public void OnStampFinished()
    {
        stampsPlaced++;

        if (stampsPlaced >= requiredStampCount)
        {
            StartCoroutine(FinishDocument());
        }
    }

    private IEnumerator FinishDocument()
    {
        yield return new WaitForSeconds(0.5f);

        Destroy(currentDocument);
          foreach (var o in GameObject.FindGameObjectsWithTag("stamp"))
            Destroy(o);
        currentDocument = null;
    }

    private IEnumerator DocumentRoutine()
{
    // セリフ（入場）
    uiManager.ShowDialogue(activePerson.enterLine);
    yield return new WaitForSeconds(1.0f);
    uiManager.ClearDialogue();

    foreach (int index in activePerson.relatedDocumentIndices)
    {
        LoadDocument(activePerson, index);

        yield return new WaitUntil(() => currentDocument == null);

        yield return new WaitForSeconds(0.4f);
    }

    // セリフ（退場）
    uiManager.ShowDialogue(activePerson.exitLine);
    yield return new WaitForSeconds(1.0f);
    uiManager.ClearDialogue();

    // GameManager へ通知（次の人へ）
    FindObjectOfType<GameManager>().OnPersonFinished();
}

}