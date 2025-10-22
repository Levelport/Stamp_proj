using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class tempo : MonoBehaviour
{ }
   /* [SerializeField] private Transform targetObject;
    [SerializeField] private GameObject stampPrefab;
    [SerializeField] private DocumentManager documentManager; // DocumentManagerへの参照

    private InnerZoneDetector2D[] innerZones;
    private Transform outerZoneTransform;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isDragging = false;
    private float rotationZ = 0f;
    private int longPressCount = 0;
    private float pressTime = 0f;
    private float longPressDuration = 0.5f;
    private bool isPressing = false;

    private Vector2 previousInputPosition;
    private Camera mainCamera;

    private int stampCount = 0;

    public void SetStampZones(InnerZoneDetector2D[] zones, Transform outer)
    {
        innerZones = zones;
        outerZoneTransform = outer;
        stampCount = 0; // ハンコ回数リセット
    }

    void Start()
    {
        originalPosition = targetObject.position;
        originalRotation = targetObject.rotation;
        originalScale = targetObject.localScale;
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector2 inputPosition = Vector2.zero;
        bool inputStarted = false;
        bool inputEnded = false;
        bool inputHeld = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        var mouse = Mouse.current;
        if (mouse != null)
        {
            inputPosition = mouse.position.ReadValue();
            inputStarted = mouse.leftButton.wasPressedThisFrame;
            inputEnded = mouse.leftButton.wasReleasedThisFrame;
            inputHeld = mouse.leftButton.isPressed;
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            inputPosition = touch.position;
            switch (touch.phase)
            {
                case TouchPhase.Began: inputStarted = true; break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled: inputEnded = true; break;
                default: inputHeld = true; break;
            }
        }
#endif

        if (inputStarted)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
            worldPos.z = 0f;
            Collider2D col = Physics2D.OverlapPoint(worldPos);
            if (col != null && col.transform == targetObject)
            {
                isPressing = true;
                pressTime = 0f;
                previousInputPosition = inputPosition;
            }
        }

        if (inputEnded)
        {
            isPressing = false;
            isDragging = false;
            targetObject.localScale = originalScale;

            longPressCount++;

            if (longPressCount >= 3)
            {
                longPressCount = 0;
                TryPlaceStamp();
                targetObject.position = originalPosition;
                targetObject.rotation = originalRotation;
            }
        }

        if (isPressing && !isDragging && inputHeld)
        {
            pressTime += Time.deltaTime;
            if (pressTime >= longPressDuration)
                isDragging = true;
        }

        if (isDragging && inputHeld)
        {
            Vector2 delta = inputPosition - previousInputPosition;

            switch (longPressCount)
            {
                case 0:
                    rotationZ -= delta.x * 0.2f;
                    targetObject.rotation = Quaternion.Euler(0f, 0f, rotationZ);
                    targetObject.localScale = originalScale * 1.3f;
                    break;
                case 1:
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputPosition);
                    worldPos.z = -1f;
                    targetObject.position = worldPos;
                    targetObject.localScale = originalScale * 1.3f;
                    break;
            }

            previousInputPosition = inputPosition;
        }
    }

    private void TryPlaceStamp()
    {
        if (stampCount >= innerZones.Length)
        {
            Debug.Log("ハンコ回数上限 reached");
            return;
        }

        Vector3 pos = targetObject.position;
        Quaternion rot = targetObject.rotation;

        float outerAngle = outerZoneTransform.eulerAngles.z;
        float stampAngle = rot.eulerAngles.z;
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(outerAngle, stampAngle));

        bool validStamp = false;

        foreach (var zone in innerZones)
        {
            Collider2D col = zone.GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(pos))
            {
                bool accepted = zone.RegisterStamp(angleDiff);
                if (accepted)
                {
                    Instantiate(stampPrefab, pos, rot);
                    stampCount++;
                    Debug.Log($"✅ ハンコ記録: angleDiff={angleDiff}, zone={zone.name}");

                    if (stampCount >= innerZones.Length)
                    {
                        Debug.Log("📄 数秒後に次の書類へ移動");

                        // 1.5秒のディレイ後に次のドキュメントをロード
                        StartCoroutine(DelayedLoadNextDocument(1.5f));
                    }
                }
                else
                {
                    Debug.Log("⚠ このゾーンには既により正確なハンコがある");
                }
                validStamp = true;
                break;
            }
        }

        if (!validStamp)
        {
            Vector3 stampPos = pos;
            stampPos.z = -0.1f; // 書類より手前に表示

            Instantiate(stampPrefab, stampPos, rot);

            Debug.Log("❌ InnerZone外：スコアなし");
        }
    }

    private IEnumerator DelayedLoadNextDocument(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        // ハンコを削除
        GameObject[] stamps = GameObject.FindGameObjectsWithTag("stamp");
        foreach (GameObject stamp in stamps)
        {
            Destroy(stamp);
        }

        documentManager.LoadNextDocument();
    }
}*/