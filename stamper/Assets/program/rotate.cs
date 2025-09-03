using UnityEngine;
using UnityEngine.InputSystem;  // 新Input System名前空間

public class Draggable2DObjectController : MonoBehaviour
{
    [SerializeField] private Transform targetObject;

    private Vector3 originalScale;
    private bool isDragging = false;
    private float rotationZ = 0f;

    private float pressTime = 0f;
    private float longPressDuration = 0.5f;
    private bool isPressing = false;

    private Camera mainCamera;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("ターゲットオブジェクトが設定されていません！");
            enabled = false;
            return;
        }
        originalScale = targetObject.localScale;
        mainCamera = Camera.main;
    }

    void Update()
    {
        var mouse = Mouse.current;

        if (mouse == null) return; // マウスがない環境なら何もしない

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            mousePos.z = 0f;

            Collider2D col = Physics2D.OverlapPoint(mousePos);
            if (col != null && col.transform == targetObject)
            {
                isPressing = true;
                pressTime = 0f;
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            isPressing = false;
            isDragging = false;
            if (targetObject != null)
                targetObject.localScale = originalScale;
        }

        if (isPressing && !isDragging)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= longPressDuration)
            {
                isDragging = true;
            }
        }

        if (isDragging)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            mousePos.z = 0f;
            targetObject.position = mousePos;

            targetObject.localScale = originalScale * 1.3f;

            // スクロールホイールの取得（新Input System）
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                rotationZ += scroll * 5f;
                targetObject.rotation = Quaternion.Euler(0f, 0f, rotationZ);
            }
        }
    }
}
