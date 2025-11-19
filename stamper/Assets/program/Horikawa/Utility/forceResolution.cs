using UnityEngine;

public class forceResolution : MonoBehaviour
{
    public int width = 720;
    public int height = 1280;
    public bool fullscreen = false;

    void Start()
    {
#if UNITY_STANDALONE
        Screen.SetResolution(width, height, fullscreen);
#endif
    }
}
