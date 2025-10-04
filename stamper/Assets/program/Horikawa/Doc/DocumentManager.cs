using UnityEngine;
using System.Collections.Generic;

public class DocumentManager : MonoBehaviour
{
    [Header("Documents")]
    [SerializeField] private GameObject[] documentPrefabs;
    [SerializeField] private Transform spawnPoint;

    [Header("Stamp")]
    [SerializeField] private Draggable2DObjectController stampController;

    [Header("People")]
    [SerializeField] private PersonData[] personDatas;
    [SerializeField] private GameObject personPrefab;
    [SerializeField] private Transform personSpawnPoint;

    private GameObject currentDocument;
    private int currentDocumentIndex = 0;

    private List<PersonController> activePeople = new List<PersonController>();

    void Start()
    {
        LoadNextDocument();
    }

    public void LoadNextDocument()
    {
        // 現在のドキュメント削除＆関連人物削除
        if (currentDocument != null)
        {
            Destroy(currentDocument);
            ClearPeople();
        }

        // 新規ドキュメント生成
        currentDocument = Instantiate(documentPrefabs[currentDocumentIndex], spawnPoint.position, Quaternion.identity);

        // スタンプ配置ゾーン取得＆セット
        InnerZoneDetector2D[] innerZones = currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();
        stampController.SetStampZones(innerZones, currentDocument.transform);

        // ドキュメントに紐づく人を出現
        SpawnRelatedPeople(currentDocumentIndex);

        // インデックス更新（ループ）
        currentDocumentIndex = (currentDocumentIndex + 1) % documentPrefabs.Length;
    }

    private void SpawnRelatedPeople(int docIndex)
    {
        ClearPeople();

        foreach (var data in personDatas)
        {
            if (System.Array.Exists(data.relatedDocumentIndices, index => index == docIndex))
            {
                GameObject go = Instantiate(personPrefab, personSpawnPoint.position, Quaternion.identity);
                PersonController person = go.GetComponent<PersonController>();
                person.Init(data, OnPersonAngry);
                activePeople.Add(person);
            }
        }
    }

    private void ClearPeople()
    {
        foreach (var person in activePeople)
        {
            if (person != null)
                Destroy(person.gameObject);
        }
        activePeople.Clear();
    }

    private void OnPersonAngry()
    {
        Debug.Log("😡 人物が怒った！ゲームオーバー処理へ");

        // 例：ゲームオーバー演出やリトライ画面表示へ
        // 今回は次の書類へ遷移
        LoadNextDocument();
    }
}
