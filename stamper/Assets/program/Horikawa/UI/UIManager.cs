using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Text UI Elements")]
    [SerializeField] private TMP_Text stampInfoText;
    [SerializeField] private TMP_Text currentStampText;
    [SerializeField] private TMP_Text remainingStampsText;
    [SerializeField] private TMP_Text remainingPeopleText;
    [SerializeField] private TMP_Text dialogueText;

    private int totalPeople;
    private int currentPersonIndex;

    public void Initialize(int total)
    {
        totalPeople = total;
        currentPersonIndex = 1;
        UpdateRemainingPeople();
        ClearDialogue();
    }

    public void UpdateStampPattern(string pattern)
    {
        stampInfoText.text = $"必要スタンプ: {pattern}";
    }

    public void UpdateCurrentStamp(StampType type)
    {
        currentStampText.text = $"現在のハンコ: {type}";
    }

    public void UpdateRemainingStamps(int remaining)
    {
        remainingStampsText.text = $"残り押印回数: {remaining}";
    }

    public void UpdateRemainingPeople()
    {
        int remaining = totalPeople - currentPersonIndex + 1;
        remainingPeopleText.text = $"残り人数: {remaining}";
    }

    public void NextPerson()
    {
        currentPersonIndex++;
        UpdateRemainingPeople();
    }

    public void ShowDialogue(string text)
    {
        dialogueText.text = text;
    }

    public void ClearDialogue()
    {
        dialogueText.text = "";
    }
}
