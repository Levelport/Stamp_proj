using UnityEngine;
using System.Collections.Generic;

public class DocumentManager : MonoBehaviour
{
    [Header("Documents")]
    [SerializeField] private GameObject[] documentPrefabs;
    [SerializeField] private Transform spawnPoint;

    [Header("Stamp")]
    [SerializeField] private Draggable2DObjectController stampController;

    [Header("People")]
    [SerializeField] private PersonData[] personDatas;
    [SerializeField] private GameObject personPrefab;
    [SerializeField] private Transform personSpawnPoint;

    private GameObject currentDocument;
    private int currentDocumentIndex = 0;

    private List<PersonController> activePeople = new List<PersonController>();
    private Dictionary<PersonData, List<int>> personScores = new Dictionary<PersonData, List<int>>();
    private int totalScore = 0;

    void Start()
    {
        LoadNextDocument();
    }

    public List<StampType> GetRequiredStamps()
    {
        // 仮の設定（例：丸→四角の順に押す必要がある）
        return new List<StampType> { StampType.Circle, StampType.Square };
    }

    public void AddScore(int score)
    {
        totalScore += score;
        Debug.Log($"📊 現在スコア: {totalScore}");
    }

    public void ResetScore()
    {
        totalScore = 0;
    }

    public void LoadNextDocument()
    {
        // 現在のドキュメント削除＆関連人物削除
        if (currentDocument != null)
        {
            Destroy(currentDocument);
            ClearPeople();
        }

        // 書類切り替え時に全スタンプ削除
        GameObject[] stamps = GameObject.FindGameObjectsWithTag("stamp");
        foreach (GameObject stamp in stamps)
            Destroy(stamp);

        // 新規ドキュメント生成
        currentDocument = Instantiate(documentPrefabs[currentDocumentIndex], spawnPoint.position, Quaternion.identity);

        // スタンプゾーン取得＆セット
        InnerZoneDetector2D[] innerZones = currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();
        stampController.SetStampZones(innerZones, currentDocument.transform);

        // 関連人物をスポーン
        SpawnRelatedPeople(currentDocumentIndex);

        // 書類評価初期化
        ResetScore();

        // インデックス更新（ループ）
        currentDocumentIndex = (currentDocumentIndex + 1) % documentPrefabs.Length;
    }

    private void SpawnRelatedPeople(int docIndex)
    {
        ClearPeople();

        foreach (var data in personDatas)
        {
            if (System.Array.Exists(data.relatedDocumentIndices, index => index == docIndex))
            {
                GameObject go = Instantiate(personPrefab, personSpawnPoint.position, Quaternion.identity);
                PersonController person = go.GetComponent<PersonController>();
                person.Init(data, OnPersonAngry);
                activePeople.Add(person);

                if (!personScores.ContainsKey(data))
                    personScores[data] = new List<int>();
            }
        }
    }

    private void ClearPeople()
    {
        foreach (var person in activePeople)
        {
            if (person != null)
                Destroy(person.gameObject);
        }
        activePeople.Clear();
    }

    private void OnPersonAngry()
    {
        Debug.Log("😡 人物が怒った！ゲームオーバー処理へ");
        EvaluateAllPersons(); // ゲームオーバー時も評価して終了扱い
    }

    // 各書類完了時に呼ばれる
    public void OnDocumentCompleted()
    {
        // 関連人物にスコア登録
        foreach (var person in activePeople)
        {
            PersonData data = GetPersonDataByController(person);
            if (data != null)
                personScores[data].Add(totalScore);
        }

        Debug.Log($"✅ 書類完了 スコア: {totalScore}");
        CheckIfAllDocumentsDone();
    }

    private PersonData GetPersonDataByController(PersonController controller)
    {
        foreach (var data in personDatas)
        {
            if (controller.name.Contains(data.name))
                return data;
        }
        return null;
    }

    private void CheckIfAllDocumentsDone()
    {
        // 全ドキュメント処理済みか？
        if (currentDocumentIndex == 0)
        {
            EvaluateAllPersons();
        }
        else
        {
            LoadNextDocument();
        }
    }

    private void EvaluateAllPersons()
    {
        float totalAverage = 0f;
        int personCount = 0;

        foreach (var kv in personScores)
        {
            float avg = 0f;
            if (kv.Value.Count > 0)
            {
                foreach (int s in kv.Value)
                    avg += s;
                avg /= kv.Value.Count;
            }

            bool passed = avg >= 70f;
            Debug.Log($"🧍 {kv.Key.name}: 平均 {avg:F1}点 → {(passed ? "可" : "不可")}");
            totalAverage += avg;
            personCount++;
        }

        if (personCount > 0)
        {
            float stageAvg = totalAverage / personCount;
            if (stageAvg >= 80f)
            {
                Debug.Log($"🎉 ステージクリア！（平均 {stageAvg:F1}点）→ ステージセレクトへ");
                // TODO: SceneManager.LoadScene("StageSelect");
            }
            else
            {
                Debug.Log($"😢 ステージ失敗（平均 {stageAvg:F1}点）→ タイトルへ戻る");
                // TODO: SceneManager.LoadScene("Title");
            }
        }
    }
}
