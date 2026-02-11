using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TitleUIManager : MonoBehaviour
{
    public GameObject optionsMenuUI;
    public GameObject BaseUI;
    public GameObject helpMenuUI;   
    public Button options;
    public Button exitoptions;

    public Button ResetButton;

    public Button StartButton;

    public Button exitHelp;    

    public Button Help;
    
    public Slider BGMSlider;
    public Slider SESlider;

    public GameObject helpui;
    public GameObject title;



    public string sceneName;

    [Header("Debug")]
    [SerializeField] private bool debugUseStageSelect = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        options.onClick.AddListener(option);
        exitHelp.onClick.AddListener(outhelp);
        Help.onClick.AddListener(inhelp);
        exitoptions.onClick.AddListener(outoption); 
       StartButton.onClick.AddListener(OnClickStart);
       ResetButton.onClick.AddListener(OnReset);


        BGMSlider.value=SoundManager_H.Instance.bgmVolume;
        SESlider.value=SoundManager_H.Instance.seVolume;

        BGMSlider.onValueChanged.AddListener(OnBGMChanged);
        SESlider.onValueChanged.AddListener(OnSEChanged);     
    }

    // Update is called once per frame
    private void option()
    {
        SoundManager_H.Instance.PlaySE("button");  
        optionsMenuUI.SetActive(true);
        BaseUI.SetActive(false);  
        title.SetActive(false);  
    }

    private void inhelp()
    {
        SoundManager_H.Instance.PlaySE("button");  
        helpMenuUI.SetActive(true);
        BaseUI.SetActive(false);
        title.SetActive(false);
        helpui.SetActive(true);
    }

    private void outhelp()
    {
       SoundManager_H.Instance.PlaySE("button");   
        helpMenuUI.SetActive(false);
        BaseUI.SetActive(true);  
        helpui.SetActive(false);
        title.SetActive(true);
    }

    private void outoption()
    {
        SoundManager_H.Instance.PlaySE("button");  
        optionsMenuUI.SetActive(false);
        BaseUI.SetActive(true);
        title.SetActive(true);
    }

    public void OnBGMChanged(float value)
    {
        SoundManager_H.Instance.SetBGMVolume(value); 
    }

    public void OnSEChanged(float value)
    {
       SoundManager_H.Instance.SetSEVolume(value);       
    }

    private void OnClickStart()
    {
        SoundManager_H.Instance.PlaySE("button");

        if (debugUseStageSelect)
        {
            // デバッグ：ステージセレクト経由
            FadeManager.Instance.FadeAndLoadScene("StageSelect");
        }
        else
        {
            // 通常：最大解放ステージを遊ぶ
            StageDataManager.Instance.SelectLatestStage();
            FadeManager.Instance.FadeAndLoadScene("Game");
        }
    }

    private void OnReset()
    {
        SoundManager_H.Instance.PlaySE("button");
        StageDataManager.Instance.ResetProgress();

    }

}
