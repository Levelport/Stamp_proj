using UnityEngine;

/// <summary>
/// Person prefab の生成と PersonController のセットアップを担当するシンプルな Manager
/// </summary>
public class PersonManager : MonoBehaviour
{
    [SerializeField] private GameObject personPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DocumentManager doc;

    private PersonController currentPerson;

    public PersonController SpawnPerson(PersonData data)
    {
        if (personPrefab == null)
        {
            Debug.LogError("PersonManager: personPrefab が未設定です");
            return null;
        }

        if (currentPerson != null)
        {
            Destroy(currentPerson.gameObject);
            currentPerson = null;
        }

        GameObject go = Instantiate(personPrefab, spawnPoint.position, Quaternion.identity);
        currentPerson = go.GetComponent<PersonController>();
        if (currentPerson == null)
        {
            Debug.LogError("PersonManager: personPrefab に PersonController がアタッチされていません");
            return null;
        }

        currentPerson.Setup(data);
        currentPerson.setDocumentManager(doc);
        return currentPerson;
    }

    public PersonController GetCurrentPerson() => currentPerson;
}
