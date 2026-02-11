using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] Image fadeImage;
    [SerializeField] float fadeDuration = 1f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutIn(sceneName));
    }

    IEnumerator FadeOutIn(string sceneName)
    {
        // フェードアウト
        yield return StartCoroutine(Fade(0, 1));

        // シーンロード
        yield return SceneManager.LoadSceneAsync(sceneName);

        // フェードイン
        yield return StartCoroutine(Fade(1, 0));
    }

    IEnumerator Fade(float start, float end)
    {
        float time = 0;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, time / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, end);
    }
}
