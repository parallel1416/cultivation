using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the Next Round button click to advance the turn.
/// Attach this to a GameObject with a Button component, or assign the button in the inspector.
/// </summary>
public class NextRoundButton : MonoBehaviour
{
    [SerializeField] private Button nextRoundButton;

    private void Awake()
    {
        // If button not assigned, try to get it from this GameObject
        if (nextRoundButton == null)
        {
            nextRoundButton = GetComponent<Button>();
        }

        if (nextRoundButton == null)
        {
            Debug.LogError("NextRoundButton: No Button component found! Please assign a button in the inspector or attach this script to a GameObject with a Button component.");
            return;
        }

        // Add listener for button click
        nextRoundButton.onClick.AddListener(OnNextRoundClicked);
    }

    private void OnDestroy()
    {
        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.RemoveListener(OnNextRoundClicked);
        }
    }

    private void OnNextRoundClicked()
    {
        // Load TurnScene instead of advancing turn
        Debug.Log("Next round button clicked - loading TurnScene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("TurnScene");
    }
}
