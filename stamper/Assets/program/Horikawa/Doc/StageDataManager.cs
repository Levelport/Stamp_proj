using UnityEngine;

public class StageDataManager : MonoBehaviour
{
    public static StageDataManager Instance { get; private set; }

    public int SelectedStage { get; private set; } = 1;   // 1～10
    public int UnlockedStage { get; private set; }        // 最大解放インデックス

    private const string KEY_UNLOCK = "UnlockedStage";
    [SerializeField] public int minStage = 1;
    [SerializeField] public int maxStage = 10;



    void Awake()
    {
       
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // 初期解放
        UnlockedStage = PlayerPrefs.GetInt(KEY_UNLOCK, 1);
        UnlockedStage = Mathf.Clamp(UnlockedStage, minStage, maxStage);
        SelectedStage = UnlockedStage;


    }

    // ステージ選択時
    public void SetStage(int stageIndex)
    {
        SelectedStage = stageIndex;
    }

    // ステージクリア時に次ステージを解放
    public void UnlockNextStage()
    {
        if (SelectedStage >= UnlockedStage)
        {
            UnlockedStage = Mathf.Clamp(SelectedStage + 1, 1, 10);
            PlayerPrefs.SetInt(KEY_UNLOCK, UnlockedStage);
            PlayerPrefs.Save();
        }
    }

    public void SelectLatestStage()
    {
        SelectedStage = UnlockedStage;
    }

    public void Promote()
    {
        if (UnlockedStage < maxStage)
        {
            UnlockedStage++;
            Save();
        }
        SelectedStage = UnlockedStage;
    }

    public void Demote()
    {
        if (UnlockedStage > minStage)
        {
            UnlockedStage--;
            Save();
        }
        SelectedStage = UnlockedStage;
    }

    private void Save()
    {
        PlayerPrefs.SetInt(KEY_UNLOCK, UnlockedStage);
        PlayerPrefs.Save();
    }

    public void ResetProgress()
{
    UnlockedStage = minStage;
    SelectedStage = minStage;

    PlayerPrefs.SetInt(KEY_UNLOCK, UnlockedStage);
    PlayerPrefs.Save();
}

}
