using UnityEngine;

public class forceResolution : MonoBehaviour
{
    public int targetWidth = 720;
    public int targetHeight = 1612;

    // ★ 追加：Standalone で強制するかどうか
    public bool forceOnStandalone = true;

    private float targetAspect;
    private int lastWidth = Screen.width;
    private int lastHeight = Screen.height;

    void Awake()
    {
        targetAspect = (float)targetWidth / targetHeight;
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        Screen.fullScreen = false;

#if UNITY_STANDALONE
        if (forceOnStandalone)
        {
            EnforceStandAloneAspect();
        }
#endif
    }

    void Update()
    {
#if UNITY_STANDALONE
        if (forceOnStandalone)
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;

                CancelInvoke(nameof(EnforceStandAloneAspect));
                Invoke(nameof(EnforceStandAloneAspect), 0.05f);
            }
        }
#endif
    }

    void EnforceStandAloneAspect()
    {
        if (Screen.fullScreen)
            Screen.fullScreen = false;

        float currentAspect = (float)Screen.width / Screen.height;

        // ほぼ同じ比率なら何もしない
        if (Mathf.Abs(currentAspect - targetAspect) < 0.01f)
            return;

        int newWidth = Screen.width;
        int newHeight = Mathf.RoundToInt(Screen.width / targetAspect);

        // 高さが画面を越える場合は幅を調整
        if (newHeight > Screen.height)
        {
            newHeight = Screen.height;
            newWidth = Mathf.RoundToInt(Screen.height * targetAspect);
        }

        Screen.SetResolution(newWidth, newHeight, false);
    }
}
