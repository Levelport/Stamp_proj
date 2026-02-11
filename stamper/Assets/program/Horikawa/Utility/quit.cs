using UnityEngine;
using UnityEngine.InputSystem;

public class QuitHandler : MonoBehaviour
{

        void Start()
    {
        if(SoundManager_H.Instance.isitPlaying() == false)
        {
        SoundManager_H.Instance.PlayBGM("bgm");
        }
    
    }
    void Update()
    {
        // --- エディタ / PC ビルド時のテスト用 ------------
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            QuitGame();
        }
#endif

        // --- Android 本番用（バックボタン） --------------
#if UNITY_ANDROID
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
        }
#endif
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
