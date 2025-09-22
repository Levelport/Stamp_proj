using UnityEngine;
using UnityEngine.UI;

public class HelpOption : MonoBehaviour
{
    [SerializeField] private Button testButton;

    private void Start()
    {
        if (testButton != null)
        {
            testButton.onClick.AddListener(OnTestButtonClick);
        }
        else
        {
            Debug.LogWarning("Test button is not assigned.");
        }
    }

    private void OnTestButtonClick()
    {
        Debug.Log("✅ Test button was clicked!");
    }
}
