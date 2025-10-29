using UnityEngine;

public class forceResolution : MonoBehaviour
{
    public int width = 310;
    public int height = 552;
    public bool fullscreen = false;

    void Start()
    {
#if UNITY_STANDALONE
        Screen.SetResolution(width, height, fullscreen);
#endif
    }
}
