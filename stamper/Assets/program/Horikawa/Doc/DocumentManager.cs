using UnityEngine;

public class DocumentManager : MonoBehaviour
{
    [SerializeField] private GameObject[] documentPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Draggable2DObjectController stampController;

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
            documentPrefabs[Random.Range(0, documentPrefabs.Length)],
            spawnPoint.position,
            Quaternion.identity
        );

        InnerZoneDetector2D[] innerZones = currentDocument.GetComponentsInChildren<InnerZoneDetector2D>();
        Transform outerZone = currentDocument.transform; // Documentのルート

        stampController.SetStampZones(innerZones, outerZone);
    }
}
