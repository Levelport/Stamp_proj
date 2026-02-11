using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private Button[] stageButtons; // 10個
    [SerializeField] private Button backToTitleButton;

    void Start()
    {
        int unlocked = StageDataManager.Instance.UnlockedStage;

        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stageIndex = i + 1;

           

            if (stageIndex <= unlocked)
            {
                stageButtons[i].interactable = true;
                stageButtons[i].onClick.AddListener(() => SelectStage(stageIndex));
            }
            else
            {
                stageButtons[i].interactable = false;
            }
        }

        backToTitleButton.onClick.AddListener(() =>
        {
            SoundManager_H.Instance.PlaySE("button");
            FadeManager.Instance.FadeAndLoadScene("Title");
        });
    }

    void SelectStage(int stageIndex)
    {
        StageDataManager.Instance.SetStage(stageIndex);
        SoundManager_H.Instance.PlaySE("button");  
        FadeManager.Instance.FadeAndLoadScene("Game");
    }
}
