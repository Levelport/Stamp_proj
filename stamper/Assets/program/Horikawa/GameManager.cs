using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private PersonManager personManager;
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private UIManager uiManager;

    private PersonData[] personDatas;
    private int currentPersonIndex = 0;
    private PersonController currentPerson;

    void Start()
    {
        LoadStageData();
        StartFirstPerson();
    }

    // --------------------------
    // ステージ CSV を読み込み
    // --------------------------
    private void LoadStageData()
    {
        int stageNum = StageDataManager.GetStageNumber();

        string csvName = $"Stage_{stageNum}";
        personDatas = PersonCSVLoader.LoadFromCSV(csvName).ToArray();

        uiManager.Initialize(personDatas.Length);

        // DocumentManager にセット
        documentManager.Setup(personDatas);
    }

    // --------------------------
    // 最初の人物出現
    // --------------------------
    private void StartFirstPerson()
    {
        SpawnPersonAndStartDocuments();
    }

    // --------------------------
    // 人物スポーン → 書類開始
    // --------------------------
    private void SpawnPersonAndStartDocuments()
    {
        if (currentPersonIndex >= personDatas.Length)
        {
            Debug.Log("全員捌き終わり → リザルトシーンへ");
            //uiManager.ShowResult();
            return;
        }

        // 人物スポーン
        currentPerson = personManager.SpawnPerson(personDatas[currentPersonIndex]);

        // DocumentManager に人物の怒りメーター参照を渡す
        documentManager.SetCurrentPerson(currentPerson);

        // 書類処理開始
        documentManager.StartProcessForPerson(personDatas[currentPersonIndex]);
    }

    // --------------------------
    // DocumentManager が呼ぶ「次の人へ」
    // --------------------------
    public void OnPersonFinished()
    {
        currentPersonIndex++;

        uiManager.NextPerson();

        SpawnPersonAndStartDocuments();
    }
}