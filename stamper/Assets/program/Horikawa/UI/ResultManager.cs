using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject result;
    public TMP_Text resultText;
    public TMP_Text scoreText;

    public GameObject goodback;
    public GameObject badback;    
    public GameObject okback;

    public GameObject Allclear;
    [Header("Buttons")]
    public GameObject titleButtonPos;
    public GameObject titlebutPosA;
    public GameObject titlebutposB;
    public Button titleButton;
    public GameObject stageselect;
    public Button stageSelectButton;

    public Sprite imageA;
    public Sprite imageB;

    public Sprite imageDebug;

    [Header("Debug")]
[SerializeField] private bool debugUseStageSelect = false;

    void Start()
    {
        titleButtonPos.transform.SetParent(titlebutPosA.transform,false);
        
        result.SetActive(true);
        float score = PlayerPrefs.GetFloat("ResultScore", 0);
        string grade ;

        if(score>=85)
        {
            goodback.SetActive(true);
            grade="良";
            SoundManager_H.Instance.PlaySE("stageclear",1.5f);
        }
        else if(score>=75)
        {
            okback.SetActive(true);
            grade="可";
            SoundManager_H.Instance.PlaySE("stageclear",1.5f);
        }
        else
        {
            badback.SetActive(true);
            grade= "不可";
            SoundManager_H.Instance.PlaySE("stagefail");
        }
        resultText.text = $"評価: {grade}";
        scoreText.text = $"平均スコア: {score:F1}点";

        // ===== 昇格 / 降格判定 =====
        if (grade == "可" || grade == "良")
        {
            if (debugUseStageSelect)
            {
                stageSelectButton.image.sprite=imageB;
            }
            else
            {
                stageSelectButton.image.sprite=imageA;
            }

            if(StageDataManager.Instance.SelectedStage >= StageDataManager.Instance.maxStage)
            {
                okback.SetActive(false);
                goodback.SetActive(false);
                result.SetActive(false);
                Allclear.SetActive(true);
                stageselect.SetActive(false);
                titleButtonPos.transform.SetParent(titlebutposB.transform,false);
            }
            else
            {
                StageDataManager.Instance.Promote();
            }
        }
        else
        {

            stageSelectButton.image.sprite=imageB;
            //StageDataManager.Instance.Demote();
            
        }
        titleButton.onClick.AddListener(OnReturnToTitle);
        if (debugUseStageSelect)
        {
            stageSelectButton.onClick.AddListener(OnStageSelect);
        }
        else
        {
            stageSelectButton.onClick.AddListener(OnContinuePlay);
        }

    }

    public void OnReturnToTitle()
    {
        SoundManager_H.Instance.PlaySE("button");
        FadeManager.Instance.FadeAndLoadScene("title");
    }

    public void OnStageSelect()
    {
        SoundManager_H.Instance.PlaySE("button");        
        FadeManager.Instance.FadeAndLoadScene("StageSelect");
    }

    public void OnContinuePlay()
    {
        SoundManager_H.Instance.PlaySE("button");
        FadeManager.Instance.FadeAndLoadScene("Game");
    }

}
