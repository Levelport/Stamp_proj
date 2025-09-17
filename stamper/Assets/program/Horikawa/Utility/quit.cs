using UnityEngine;
using UnityEngine.InputSystem;

public class quit : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            QuitGame();
        }
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
