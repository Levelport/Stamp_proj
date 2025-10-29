using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DocumentManager : MonoBehaviour
{
    [Header("Documents")]
    [SerializeField] private GameObject[] documentPrefabs; // 書類プレハブ（bl_Paper等）
    [SerializeField] private Transform spawnPoint;         // 書類出現位置

    [Header("Stamp Controller")]
    [SerializeField] public StampOperatorController stampController; // ハンコ操作担当

    [Header("UI")]
    [SerializeField] public UIManager uiManager;

    private PersonData[] personDatas;
    private GameObject currentDocument;
    private int currentPersonIndex = 0;

    private float totalScore = 0f;
    private int totalDocuments = 0;

    void Start()
    {
        // ステージCSV読み込み
        int stageNum = StageDataManager.GetStageNumber();
        string csvName = $"Stage_{stageNum}";
        personDatas = PersonCSVLoader.LoadFromCSV(csvName).ToArray();

        uiManager.Initialize(personDatas.Length);
        StartCoroutine(ProcessPeopleRoutine());
    }

    /// <summary>
    /// 各人物の処理（セリフ→書類→スコア計算）
    /// </summary>
    private IEnumerator ProcessPeopleRoutine()
    {
        foreach (var person in personDatas)
        {
            // 入場セリフ
            uiManager.ShowDialogue(person.enterLine);
            yield return new WaitForSeconds(1.2f);
            uiManager.ClearDialogue();

            // 関連書類を順に処理
            foreach (int docIndex in person.relatedDocumentIndices)
            {
                LoadDocument(person, docIndex);

                // 書類完了待ち（InnerZoneDetector2Dが通知するまで）
                yield return new WaitUntil(() => currentDocument == null);

                yield return new WaitForSeconds(0.5f);
            }

            // 退場セリフ
            uiManager.ShowDialogue(person.exitLine);
            yield return new WaitForSeconds(1.2f);
            uiManager.ClearDialogue();
            uiManager.NextPerson();
        }

        // スコア平均を算出
        float averageScore = totalDocuments > 0 ? totalScore / totalDocuments : 0f;
        PlayerPrefs.SetFloat("ResultScore", averageScore);

        // ステージ結果へ
        SceneManager.LoadScene("ResultScene");
    }

    /// <summary>
    /// 書類の生成（前の書類を削除して新規ロード）
    /// </summary>
    private void LoadDocument(PersonData person, int docIndex)
    {
        // 既存ドキュメント破棄＆スタンプ削除
        if (currentDocument != null)
            Destroy(currentDocument);
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("stamp"))
            Destroy(g);

        // 書類角度を決定
        float angle = (person.docAngleMode == "Random")
            ? Random.Range(person.docAngleMin, person.docAngleMax)
            : person.docAngleMin;

        currentDocument = Instantiate(documentPrefabs[docIndex],
            spawnPoint.position, Quaternion.Euler(0, 0, angle));

        // 必要スタンプ種類を解析してUI更新
        List<StampType> requiredStamps = GetRequiredStampsFromPattern(person.stampPattern);
        uiManager.UpdateStampPattern(person.stampPattern);
        uiManager.UpdateRemainingStamps(requiredStamps.Count);

        // InnerZoneにDocumentManagerを登録（スコア通知用）
        InnerZoneDetector2D[] zones = currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();
        foreach (var zone in zones)
            zone.SetManager(this, requiredStamps);
    }

    /// <summary>
    /// InnerZoneDetector2D から呼ばれる：書類完了
    /// </summary>
    public void OnDocumentCompleted(float documentScore)
    {
        totalScore += documentScore;
        totalDocuments++;

        if (currentDocument != null)
        {
            Destroy(currentDocument);
            currentDocument = null;
        }
    }

    /// <summary>
    /// CSVパターン（例："Circle:1|Square:1"）を分解してStampTypeリストを返す
    /// </summary>
    public List<StampType> GetRequiredStampsFromPattern(string pattern)
    {
        List<StampType> result = new List<StampType>();
        if (string.IsNullOrEmpty(pattern)) return result;

        string[] parts = pattern.Split('|');
        foreach (string p in parts)
        {
            string[] typeCount = p.Split(':');
            if (typeCount.Length != 2) continue;

            if (System.Enum.TryParse(typeCount[0], out StampType type))
            {
                if (int.TryParse(typeCount[1], out int count))
                {
                    for (int i = 0; i < count; i++)
                        result.Add(type);
                }
            }
        }
        return result;
    }
}
