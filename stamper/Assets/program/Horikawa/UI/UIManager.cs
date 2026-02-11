using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    [Header("Text UI")]
    public TMP_Text dialogueText;
    public TMP_Text remainingPeopleText;
    public Image background;

    public TMP_Text currentModeText;
    public TMP_Text RankText;
    public TMP_Text currentStampText;
    public TMP_Text playerRoleText;



    public Slider BGMSlider;
    public Slider SESlider;

    private int totalPeople;
    private int currentIndex = 0;

    public void Initialize(int peopleCount,int stagenum)
    {
        totalPeople = peopleCount;
        currentIndex = 0;
        UpdateRemainingPeople();
        ClearDialogue();
        SetRank(stagenum);


        BGMSlider.value=SoundManager_H.Instance.bgmVolume;
        SESlider.value=SoundManager_H.Instance.seVolume;

        BGMSlider.onValueChanged.AddListener(OnBGMChanged);
        SESlider.onValueChanged.AddListener(OnSEChanged);


        if (currentStampText != null)
            currentStampText.text = "ハンコ：なし";

        if (currentModeText != null)
            currentModeText.text = "モード：なし";
    }

    public void OnBGMChanged(float value)
    {
        SoundManager_H.Instance.SetBGMVolume(value);
    }

    public void OnSEChanged(float value)
    {
        SoundManager_H.Instance.SetSEVolume(value); 
    }
    // ---------------------
    // 人数表示
    // ---------------------
    public void NextPerson()
    {
        currentIndex++;
        UpdateRemainingPeople();
    }

    private void UpdateRemainingPeople()
    {
        if (remainingPeopleText != null)
            remainingPeopleText.text = $"残り人数：{(totalPeople - currentIndex)}人";
    }

    // ---------------------
    // Dialogue
    // ---------------------
    public void ShowDialogue(string line)
    {
        if (dialogueText != null)
            dialogueText.text = line;
            background.enabled=true;
    }

    public void ClearDialogue()
    {
        if (dialogueText != null)
            dialogueText.text = "";
            background.enabled=false;
    }

    // ---------------------
    // StampOperatorController が呼ぶ
    // ---------------------
    public void UpdateCurrentStamp(StampType? type)
    {
        if (currentStampText == null) return;

        if (type == null)
            currentStampText.text = "ハンコ：なし";
        else
            currentStampText.text = $"操作ハンコ：{type.ToString()}";
    }

    public void UpdateOperationMode(string mode)
    {
        if (currentModeText != null)
            currentModeText.text = $"モード：\n{mode}";
    }

    public void SetRank(int stageNum)
    {
        string ranks="null";
        switch  (stageNum)
        {
        case 1:
            ranks="係長";
            RankText.text=ranks;
        break;
        case 2:
            ranks="課長";
            RankText.text=ranks;
        break;
        case 3:
            ranks="部長";
            RankText.text=ranks;
        break;
        case 4:
            ranks="常務";
            RankText.text=ranks;
        break;
        case 5:
        case 6:
            ranks="副社長";
            RankText.text=ranks;
        break;
        default:
            ranks="社長";
            RankText.text=ranks;
        break;
        }


    }

}
