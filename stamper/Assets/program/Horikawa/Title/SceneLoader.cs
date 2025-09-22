using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Button targetButton;  // ヒエラルキー上のボタンを割り当てる
    [SerializeField] private string sceneName;     // 遷移先のシーン名

    private void Start()
    {
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(() => LoadSceneByName(sceneName));
        }
        else
        {
            Debug.LogWarning("Button not assigned.");
        }
    }

    public void LoadSceneByName(string name)
    {
        SceneManager.LoadScene(name);
    }
}
