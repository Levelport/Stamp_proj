using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DocumentManager : MonoBehaviour
{
    [Header("Document Prefabs")]
    [SerializeField] private GameObject[] documentPrefabs;
    [SerializeField] private Transform spawnPoint;

    // GameManager からセットされる
    private PersonManager personManager;
    private UIManager uiManager;

    // ステージデータ（GameManager → BeginProcess で受け取る）
    private PersonData[] personDatas;

    // 現在処理中
    private GameObject currentDocument;
    private PersonController currentPerson;

    public float dialogSec=1.75f;

    public float outdialogSec=2.0f;

    // スコア関連
    private float totalScore = 0f;
    private int totalDocuments = 0;

    // その書類で必要なスタンプ数と押した数
    private int requiredStampCount = 0;
    private int stampsPlaced = 0;

    private bool angerQuit=false;

    // ===== 書類単位スコア =====
    private float documentScore = 0f;
    private int documentStampCount = 0;


    // ========================
    // GameManager から呼ばれる
    // ========================
    public void SetPersonManager(PersonManager pm)
    {
        personManager = pm;
    }

    public void SetUIManager(UIManager ui)
    {
        uiManager = ui;
    }

    public void BeginProcess(PersonData[] datas)
    {
        personDatas = datas;
        StartCoroutine(ProcessPeopleRoutine());
    }

    // 互換用（呼ばれても何もしない）
    public void InitializeIfNeeded() { }

    // ========================
    // メインの流れ：人ごとに書類を処理
    // ========================
    private IEnumerator ProcessPeopleRoutine()
    {
        if (personDatas == null || personDatas.Length == 0)
        {
            Debug.LogWarning("DocumentManager: personDatas が空です");
            yield break;
        }

        foreach (var person in personDatas)
        {
            angerQuit=false;
            // 人を出す
            currentPerson = personManager.SpawnPerson(person);

            // 入場セリフ
            if (uiManager != null)
            {
                uiManager.ShowDialogue(person.enterLine);
            }
            yield return new WaitForSeconds(dialogSec);
            uiManager?.ClearDialogue();

            // この人の書類を順番に処理
            int docOrder = 0;
            foreach (int docIndex in person.relatedDocumentIndices)
            {
                if(angerQuit)
                {
                    int remainingDocs=person.relatedDocumentIndices.Length-docOrder;
                    for(int i=0;i<remainingDocs;i++)
                    {
                        totalScore+=0;
                        totalDocuments++;
                    }
                    break;
                }

                LoadDocument(person, docIndex, docOrder);

                // この書類(currentDocument)が null になるまで待つ
                yield return new WaitUntil(() => currentDocument == null);

                // 少し間を空ける（セリフなど用）
                yield return new WaitForSeconds(1.0f);

                docOrder++;
            }

            if(!angerQuit)
            {
            // 退場セリフ
                if (uiManager != null)
                {
                    uiManager.ShowDialogue(person.exitLine);
                }
                yield return new WaitForSeconds(outdialogSec);
                uiManager?.ClearDialogue();
            }   
            yield return new WaitForSeconds(0.75f);

            // 残り人数更新
            uiManager?.NextPerson();
        }

        // すべての人物終了 → 平均スコアを保存してリザルトへ
        float averageScore = (totalDocuments > 0) ? (totalScore / totalDocuments) : 0f;
        PlayerPrefs.SetFloat("ResultScore", averageScore);


        FadeManager.Instance.FadeAndLoadScene("ResultScene");
    }

    // ========================
    // 書類生成＋InnerZone へ必要スタンプ割り当て
    // ========================
    private void LoadDocument(PersonData person, int docIndex, int docOrder)
    {
        // 既存書類削除
        if (currentDocument != null)
        {
            Destroy(currentDocument);
            currentDocument = null;
        }

        // 既存スタンプ削除
        foreach (GameObject s in GameObject.FindGameObjectsWithTag("stamp"))
        {
            Destroy(s);
        }

        // 角度決定
        float angle = person.docAngleMode != null && person.docAngleMode.ToLower() == "random"
            ? Random.Range(person.docAngleMin, person.docAngleMax)
            : person.docAngleMin;

        if (docIndex < 0 || docIndex >= documentPrefabs.Length)
        {
            Debug.LogError($"DocumentManager: docIndex が範囲外です ({docIndex})");
            currentDocument = null;
            requiredStampCount = 0;

            return;
        }

        currentDocument = Instantiate(
            documentPrefabs[docIndex],
            spawnPoint.position,
            Quaternion.Euler(0, 0, angle)
        );

        // この人の stampPattern から、この書類(docOrder)用のパターンを取り出す
        Dictionary<string, StampType> pattern = ParsePatternForDocument(person.stampPattern, docOrder);

        // InnerZone を取得して割り当て
        InnerZoneDetector2D[] zones = currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();
        foreach (var zone in zones)
        {
            zone.isStamped = false;

            string key = (zone.zoneID ?? "").Trim().ToUpperInvariant();

            if (pattern.TryGetValue(key, out StampType t))
            {
                zone.requiredType = t;
            }
            else
            {
                // この書類では使わないゾーン
                zone.isStamped = true;
            }
        }

        requiredStampCount = pattern.Count;
        stampsPlaced = 0;

        currentPerson?.StartAnger();
        // ★ 書類スコア初期化
        documentScore = 0f;
        documentStampCount = 0;

    }

    /// <summary>
    /// StampPattern 文字列から、この書類(docOrder)に対応する zoneID→StampType のマップを返す。
    /// 例: "BL:Circle|CR:Square|TR:Circle"
    ///   docOrder=0 → "BL:Circle"
    ///   docOrder=1 → "CR:Square"
    /// </summary>
    private Dictionary<string, StampType> ParsePatternForDocument(string pattern, int docOrder)
    {
        Dictionary<string, StampType> dict = new Dictionary<string, StampType>();

        if (string.IsNullOrEmpty(pattern))
            return dict;

        // 書類ごとに '|' で区切る
        string[] docParts = pattern.Split('|');

        if (docParts.Length == 0)
            return dict;

        // docOrder が配列長を超えていたら最後の要素を使う
        int idx = Mathf.Clamp(docOrder, 0, docParts.Length - 1);
        string docPart = docParts[idx].Trim();

        if (string.IsNullOrEmpty(docPart))
            return dict;

        // "TR:Square&BR:Circle" のような複数指定も想定
        string[] pairs = docPart.Split('&');
        foreach (string p in pairs)
        {
            if (string.IsNullOrWhiteSpace(p)) continue;

            string[] kv = p.Split(':');
            if (kv.Length != 2) continue;

            string zone = kv[0].Trim().ToUpperInvariant();
            string typeStr = kv[1].Trim();

            if (System.Enum.TryParse(typeStr, true, out StampType type))
            {
                if (!dict.ContainsKey(zone))
                    dict.Add(zone, type);
            }
        }

        return dict;
    }

    // ========================
    // StampOperatorController から呼ばれる押印完了
    // doc.OnStampPlaced(stamped); で呼ぶことを想定
    // ========================
    public void OnStampPlaced(GameObject stampObj)
    {
        if (currentDocument == null)
        {
            Debug.LogWarning("DocumentManager: currentDocument が null の状態で OnStampPlaced が呼ばれました");
            Destroy(stampObj);
            return;
        }

        InnerZoneDetector2D[] zones = currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();

        if (zones == null || zones.Length == 0)
        {
            // 書類にゾーンが無い → 書類外扱い
            Debug.Log("DocumentManager: ゾーン無し書類に押印 -20点");
            totalScore -= 20f;
            totalDocuments++;
            
            documentScore -= 20f;
            documentStampCount++;

            // スタンプオブジェクト自体は残すかどうかは好みだが、今は残す
            return;
        }

        // 未使用ゾーンの中から、最も近いものを探す
        InnerZoneDetector2D nearest = null;
        float nearestDist = float.MaxValue;

        Vector3 stampPos = stampObj.transform.position;

        foreach (var z in zones)
        {
            if (z.isStamped) continue; // もう使ったゾーンはスキップ

            float d = Vector3.Distance(stampPos, z.Center);
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = z;
            }
        }

        if (nearest == null)
        {
            // すべて使用済みゾーンから外れた押印 → 書類外扱い
            Debug.Log("DocumentManager: すべてのゾーン使用済み後の押印 -15点");
            totalScore -= 15f;
            totalDocuments++;

            documentScore -= 15f;
            documentStampCount++;

            return;
        }

        // スコア計算（内部で isStamped = true になる）
        float score = nearest.CalculateScore(stampObj);
        nearest.MarkStamped();

        documentScore += score;
        documentStampCount++;  

        // ★ ゾーン単位のSE分岐
        if (score < 75f)
        {
            SoundManager_H.Instance.PlaySE("stamp_bad", 2f);
        }
        else
        {
            SoundManager_H.Instance.PlaySE("stamp_good", 5f);
        }

        Debug.Log($"DocumentManager: 押印 zone={nearest.zoneID} score={score:F1}");

        totalScore += score;
        totalDocuments++;

        // 正しいゾーンに対しての押印なので、書類進行用カウント
        stampsPlaced++;


        // 必要数押したらこの書類は完了
        if (stampsPlaced >= requiredStampCount)
        {
            StartCoroutine(FinishDocumentRoutine());
        }
    }

    // ========================
    // 書類完了（少し待ってから切り替え）
    // ========================
   private IEnumerator FinishDocumentRoutine()
{
    currentPerson?.StopAnger();

    float avg = (documentStampCount > 0)
        ? (documentScore / documentStampCount)
        : 0f;

    yield return new WaitForSeconds(0.75f);

    // ★ 書類評価によるセリフ分岐
    if (uiManager != null)
    {
        if (avg < 75f)
        {
            uiManager.ShowDialogue("……ハンコが少しズレてますね。");
        }
        else
        {
            uiManager.ShowDialogue("はい、問題ありません。");
        }
    }

    yield return new WaitForSeconds(1.25f);
    uiManager?.ClearDialogue();

    if (currentDocument != null)
    {
        Destroy(currentDocument);
        currentDocument = null;
    }

    foreach (GameObject s in GameObject.FindGameObjectsWithTag("stamp"))
    {
        Destroy(s);
    }
}

 
    public void OnAngerTimeout()
    {
        if(angerQuit)return;
        angerQuit=true;
        StartCoroutine(AngerTimeoutRoutine());
    }

    private IEnumerator AngerTimeoutRoutine()
    {
        uiManager?.ShowDialogue("遅い、もういいです");
        yield return new WaitForSeconds(outdialogSec);
        uiManager?.ClearDialogue();

        if(currentDocument!=null)
        {
            totalScore+=0;
            totalDocuments++;
            Destroy(currentDocument);
            currentDocument=null;
        }


        foreach(GameObject s in GameObject.FindGameObjectsWithTag("stamp"))
        {
            Destroy(s);
        }


    }


}
