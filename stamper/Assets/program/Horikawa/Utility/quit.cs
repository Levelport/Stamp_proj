using UnityEngine;
using UnityEngine.InputSystem;

public class quit : MonoBehaviour
{
    void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        if (Keyboard.current.qKey.wasPressedThisFrame)
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
