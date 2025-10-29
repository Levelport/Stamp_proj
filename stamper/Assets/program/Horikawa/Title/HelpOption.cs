using UnityEngine;
using UnityEngine.UI;

public class HelpOption : MonoBehaviour
{
    [Header("Main Buttons")]
    public Button descriptionButton;
    public Button settingsButton;

    [Header("Windows")]
    public GameObject descriptionWindow;
    public GameObject settingsWindow;

    [Header("Close Buttons")]
    public Button descriptionCloseButton;
    public Button settingsCloseButton;

    void Start()
    {
        // ボタンイベント登録
        descriptionButton.onClick.AddListener(OpenDescriptionWindow);
        settingsButton.onClick.AddListener(OpenSettingsWindow);

        descriptionCloseButton.onClick.AddListener(CloseAllWindows);
        settingsCloseButton.onClick.AddListener(CloseAllWindows);

        // 初期状態でウインドウを非表示
        CloseAllWindows();
    }

    void OpenDescriptionWindow()
    {
        descriptionWindow.SetActive(true);
        settingsWindow.SetActive(false);
    }

    void OpenSettingsWindow()
    {
        settingsWindow.SetActive(true);
        descriptionWindow.SetActive(false);
    }

    void CloseAllWindows()
    {
        descriptionWindow.SetActive(false);
        settingsWindow.SetActive(false);
    }
}
