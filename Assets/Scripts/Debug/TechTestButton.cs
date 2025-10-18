using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// temporary debug button to print the current status of the tech tree to the console.
/// </summary>

public class TechTreeDebugger : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnDebugButtonClicked);
    }

    private void OnDebugButtonClicked()
    {
        if (TechManager.Instance != null)
        {
            TechManager.Instance.DebugTechTreeStatus();
        }
        else
        {
            LogController.LogError("TechManager instance is not available.");
        }
    }
}