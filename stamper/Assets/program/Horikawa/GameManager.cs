using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Managers (Inspector でセット)")]
    [SerializeField] private PersonManager personManager;
    [SerializeField] private DocumentManager documentManager;
    [SerializeField] private UIManager uiManager;


    // CSVから読み込まれたこのステージでの全員分
    private PersonData[] personDatas;

    private void Start()
    {

        LoadStageData();
        SetupManagers();
        StartGameProcess();
    }

    /// <summary>
    /// StageDataManager からステージ番号を取得し、
    /// 対応する CSV (例: Stage_1) から PersonData[] を読み込む。
    /// </summary>
private void LoadStageData()
{
    int stageNum = StageDataManager.Instance.SelectedStage;

    string csvName = $"Stage_{stageNum}";
    var list = PersonCSVLoader.LoadFromCSV(csvName);
    personDatas = list.ToArray();

    uiManager.Initialize(personDatas.Length,stageNum);
    documentManager.SetPersonManager(personManager);
    documentManager.SetUIManager(uiManager);
}


    /// <summary>
    /// DocumentManager に PersonManager / UIManager を渡して
    /// 依存関係を一元化する。
    /// </summary>
    private void SetupManagers()
    {
        if (documentManager == null)
        {
            Debug.LogError("GameManager: DocumentManager が Inspector に設定されていません。");
            return;
        }
        if (personManager == null)
        {
            Debug.LogError("GameManager: PersonManager が Inspector に設定されていません。");
            return;
        }
        if (uiManager == null)
        {
            Debug.LogError("GameManager: UIManager が Inspector に設定されていません。");
            return;
        }

        // DocumentManager 側に参照を渡す（DocumentManager に以下メソッドがある前提）
        documentManager.SetPersonManager(personManager);
        documentManager.SetUIManager(uiManager);

        // DocumentManager 側で何か初期化が必要ならこの中で行うようにしておく
        if (documentManager is IInitializableDocumentManager init)
        {
            init.Initialize();
        }
    }

    /// <summary>
    /// DocumentManager に「このステージの登場人物データ」を渡して処理開始。
    /// 書類ループや押印判定、次の人へ進める処理はすべて DocumentManager 側で行う。
    /// </summary>
    private void StartGameProcess()
    {
        if (documentManager == null)
        {
            Debug.LogError("GameManager: DocumentManager がありません。");
            return;
        }

        if (personDatas == null || personDatas.Length == 0)
        {
            Debug.LogError("GameManager: personDatas が空のため処理を開始できません。");
            return;
        }

        // DocumentManager 側にこのステージの人物データを渡してメイン処理開始
        documentManager.BeginProcess(personDatas);
    }
}

/// <summary>
/// DocumentManager 側に任意実装させるためのオプションインターフェース。
/// 必要なければ DocumentManager に実装しなくてもよい。
/// </summary>
public interface IInitializableDocumentManager
{
    void Initialize();
}
