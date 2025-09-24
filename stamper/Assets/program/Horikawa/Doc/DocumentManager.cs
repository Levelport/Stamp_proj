using UnityEngine;
using System.Collections.Generic; // Listのために必要
public class DocumentManager : MonoBehaviour
{
    [SerializeField] private GameObject[] documentPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Draggable2DObjectController stampController;

    [SerializeField] private PersonData[] personDatas;
[SerializeField] private GameObject personPrefab;
[SerializeField] private Transform personSpawnPoint;


private List<PersonController> activePeople = new List<PersonController>();
private int currentDocumentIndex = 0;


    private GameObject currentDocument;

    public void Start()
    {
        LoadNextDocument();
    }

public void LoadNextDocument()
{
    if (currentDocument != null)
        Destroy(currentDocument);

    currentDocument = Instantiate(
        documentPrefabs[currentDocumentIndex],
        spawnPoint.position,
        Quaternion.identity
    );

    InnerZoneDetector2D[] innerZones = currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();
    Transform outerZone = currentDocument.transform;
    stampController.SetStampZones(innerZones, outerZone);

    SpawnRelatedPeople(currentDocumentIndex);

    currentDocumentIndex = (currentDocumentIndex + 1) % documentPrefabs.Length;
}

private void SpawnRelatedPeople(int documentIndex)
{
    // 人物リセット
    foreach (var person in activePeople)
    {
        Destroy(person.gameObject);
    }
    activePeople.Clear();

    // 該当する人だけ出す
    foreach (var data in personDatas)
    {
        if (System.Array.Exists(data.relatedDocumentIndices, d => d == documentIndex))
        {
            GameObject go = Instantiate(personPrefab, personSpawnPoint.position, Quaternion.identity);
            PersonController controller = go.GetComponent<PersonController>();
            controller.Init(data, OnPersonAngry);
            activePeople.Add(controller);
        }
    }
}

    private void OnPersonAngry()
{
    Debug.Log("😡 人が怒った！ゲームオーバー？リトライ？");
        LoadNextDocument();
}

}
