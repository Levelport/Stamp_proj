using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    void Start()
    {
        // ターゲットFPSを30に設定
        Application.targetFrameRate = 30;

        // VSyncを無効化（有効だとVSyncが優先されてFPS制限が効かない）
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        // 現在のFPSを確認したい場合はデバッグ表示
        // Debug.Log(1.0f / Time.deltaTime);
    }
}
