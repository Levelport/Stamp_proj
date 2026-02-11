using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button pause;
    public Button unpause;

    public Button Giveup;
    [Header("Debug")]
    [SerializeField] private bool debugUseStageSelect = false;

    private void Start()
    {
        pause.onClick.AddListener(PauseGame) ;
        unpause.onClick.AddListener(ResumeGame);
        Giveup.onClick.AddListener(goback);
    }

    public void PauseGame()
    {
        SoundManager_H.Instance.PlaySE("button");       
        pauseMenuUI.SetActive(true);  
        Time.timeScale = 0f;          

    }

    public void ResumeGame()
    {
        SoundManager_H.Instance.PlaySE("button");  
        pauseMenuUI.SetActive(false); 
        Time.timeScale = 1f;          
      
    }

    private void goback()
    {
        SoundManager_H.Instance.PlaySE("button");  
        Time.timeScale = 1f;
        if (debugUseStageSelect)
        {
            FadeManager.Instance.FadeAndLoadScene("StageSelect");
        }
        else
        {
            FadeManager.Instance.FadeAndLoadScene("Title");
        }
      
    }
}
