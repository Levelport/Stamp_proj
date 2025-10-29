using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text scoreText;

    void Start()
    {
        float score = PlayerPrefs.GetFloat("ResultScore", 0);
        string grade = score >= 80 ? "可" : "不可";
        resultText.text = $"評価: {grade}";
        scoreText.text = $"平均スコア: {score:F1}点";
    }

    public void OnReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void OnStageSelect()
    {
        SceneManager.LoadScene("StageSelectScene");
    }
}
