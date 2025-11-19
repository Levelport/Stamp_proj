using UnityEngine;

public class PersonManager : MonoBehaviour
{
    [Header("Person Prefab")]
    [SerializeField] private GameObject personPrefab;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    private PersonController currentPerson;

    // DocumentManager から呼ばれる
    public PersonController SpawnPerson(PersonData data)
    {
        if (currentPerson != null)
            Destroy(currentPerson.gameObject);

        GameObject obj = Instantiate(personPrefab, spawnPoint.position, Quaternion.identity);
        currentPerson = obj.GetComponent<PersonController>();

        currentPerson.Setup(data);

        return currentPerson;
    }

    public PersonController GetCurrentPerson()
    {
        return currentPerson;
    }
}