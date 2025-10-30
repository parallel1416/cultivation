using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        // If button not assigned, try to get it from this GameObject
        if (exitButton == null)
        {
            exitButton = GetComponent<Button>();
        }

        if (exitButton == null)
        {
            Debug.LogError("ExitButton: No Button component found! Please assign a button in the inspector or attach this script to a GameObject with a Button component.");
            return;
        }

        // Add listener for button click
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(OnExitClicked);
        }
    }

    private void OnExitClicked()
    {
        Debug.Log("ExitButton: Exit button clicked - quitting application");

        // Quit the application
        Application.Quit();

        // Note: Application.Quit() does not work in the editor. To test in the editor, uncomment the following line:
        //UnityEditor.EditorApplication.isPlaying = false;
    }
}
