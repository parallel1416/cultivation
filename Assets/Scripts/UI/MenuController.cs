using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the Menu scene interactions
/// Triggers the transition to Map when player taps
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject tapToStartIndicator;

    [Header("Settings")]
    [SerializeField] private bool useButtonOrTapAnywhere = false; // false = tap anywhere, true = button only

    private bool hasStarted = false;

    private void Start()
    {
        // Setup button listener if using button
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    private void Update()
    {
        // Allow tap anywhere to start
        if (!useButtonOrTapAnywhere && !hasStarted)
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                OnPlayClicked();
            }
        }
    }

    private void OnPlayClicked()
    {
        if (hasStarted) return;
        if (SceneTransitionManager.Instance.IsTransitioning) return;

        hasStarted = true;

        Debug.Log("Starting game - transitioning to Map...");

        // Hide tap indicator
        if (tapToStartIndicator != null)
        {
            tapToStartIndicator.SetActive(false);
        }

        // Disable button
        if (playButton != null)
        {
            playButton.interactable = false;
        }

        // Trigger transition
        SceneTransitionManager.Instance.TransitionMenuToMap(2.0f, OnTransitionComplete);
    }

    private void OnTransitionComplete()
    {
        Debug.Log("Menu to Map transition complete!");
    }
}
