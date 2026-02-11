using UnityEngine;

/// <summary>
/// 人物の見た目表示＋怒りメーター（AngerFillシェーダ）を制御する
/// </summary>
public class PersonController : MonoBehaviour
{
    [Header("Face Sprite Renderer (素の顔)")]
    [SerializeField] private SpriteRenderer faceRenderer;

    [Header("Anger Sprite Renderer (赤くなる用オーバーレイ)")]
    [SerializeField] private SpriteRenderer angerRenderer;

    private DocumentManager documentManager;

    private float angerTimeMax;
    private float angerTimer;

    private bool angerActive = false;
    private PersonData myData;

    // AngerFill 用マテリアルインスタンス（個々のキャラ用）
    private Material angerMaterialInstance;



    private const string ANGER_MATERIAL_PATH = "Materials/AngerFill"; // Resources/Materials/AngerFill.mat
    private const string ANGER_FILL_PROP = "_FillAmount";

    public void Setup(PersonData data)
    {
        myData = data;


        // ---------- SpriteRenderer の自動取得フォールバック ----------
        if (faceRenderer == null)
        {
            // プレハブ側でセットしていなかった場合の保険
            faceRenderer = GetComponent<SpriteRenderer>();
            if (faceRenderer == null)
            {
                SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
                if (srs.Length > 0) faceRenderer = srs[0];
            }
        }

        // ---------- キャラスプライト設定 ----------
        if (faceRenderer != null)
        {
            // 例: Resources/Characters/shainn1 など
            Sprite s = Resources.Load<Sprite>("Characters/" + data.baseSpriteName);
            if (s != null)
            {
                faceRenderer.sprite = s;
                angerRenderer.sprite=s;
            }
            else
            {
                Debug.LogWarning($"PersonController: Sprite '{data.baseSpriteName}' 読み込み不可");
            }
        }

        // ---------- AngerFill マテリアル固定 ----------
        if (angerRenderer != null)
        {
            // Resources から AngerFill マテリアル読み込み
            Material baseMat = Resources.Load<Material>(ANGER_MATERIAL_PATH);
            if (baseMat != null)
            {
                // 共有マテリアルを書き換えないようにインスタンスを作る
                angerMaterialInstance = new Material(baseMat);
                angerRenderer.material = angerMaterialInstance;

                // 開始時は fill=0（真っ赤ゼロ）
                angerMaterialInstance.SetFloat(ANGER_FILL_PROP, 0f);
            }
            else
            {
                Debug.LogWarning("PersonController: AngerFill マテリアルが Resources/Materials に見つかりません");
            }
        }

        // ---------- 怒りタイマー ----------
        angerTimeMax = Mathf.Max(0.01f, data.angerTime); // 0割防止
        angerTimer = angerTimeMax;

        angerActive = false;
    }

    void Update()
    {
        if (!angerActive) return;

        angerTimer -= Time.deltaTime;
        if (angerTimer < 0f) angerTimer = 0f;

        // 0 → 1 に進む怒り度合
        float t = 1f - (angerTimer / angerTimeMax);   // 0:落ち着いてる / 1:MAX怒り
        t = Mathf.Clamp01(t);

        // AngerFill のフィル量を更新（下から赤くしていく）
        if (angerMaterialInstance != null)
        {
            angerMaterialInstance.SetFloat(ANGER_FILL_PROP, t);
        }

        if (angerTimer <= 0f)
        {
            angerActive = false;
            OnAngerTimeout();
        }
    }

    private void OnAngerTimeout()
    {
        Debug.Log("怒りタイマー終了 → MAX怒り状態");

        angerActive=false;
        if(documentManager!=null)
        {
            documentManager.OnAngerTimeout();
        }
        // ここで退場処理・ミス扱いなどを入れてもOK
    }

    public void ResetAnger()
    {
        angerTimer = angerTimeMax;
        angerActive = true;

        if (angerMaterialInstance != null)
        {
            angerMaterialInstance.SetFloat(ANGER_FILL_PROP, 0f);
        }
    }

    public void setDocumentManager(DocumentManager doc)
    {
        documentManager=doc;
    }

    public PersonData GetPersonData()
    {
        return myData;
    }

    public void StartAnger()
    {
        angerActive=true;
    }

    public void StopAnger()
    {
        angerActive=false;
    }
}
