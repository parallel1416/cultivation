using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor utility to batch-add UIButtonSound components to all buttons in the scene.
/// This script only works in the Unity Editor.
/// </summary>
public class ButtonSoundUtility : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Batch Add Settings")]
    [Tooltip("Add UIButtonSound to all buttons in scene")]
    [SerializeField] private bool includeInactiveButtons = true;
    
    [Header("Default Settings")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private bool playHoverSound = false;

    [ContextMenu("Add UIButtonSound to All Buttons")]
    private void AddSoundToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(includeInactiveButtons);
        int addedCount = 0;
        int skippedCount = 0;

        foreach (Button button in buttons)
        {
            // Check if button already has UIButtonSound
            if (button.GetComponent<UIButtonSound>() != null)
            {
                skippedCount++;
                continue;
            }

            // Add UIButtonSound component
            UIButtonSound buttonSound = button.gameObject.AddComponent<UIButtonSound>();
            
            // Configure default settings via SerializedObject (required for private fields)
            SerializedObject so = new SerializedObject(buttonSound);
            so.FindProperty("playClickSound").boolValue = playClickSound;
            so.FindProperty("playHoverSound").boolValue = playHoverSound;
            so.ApplyModifiedProperties();

            addedCount++;
            EditorUtility.SetDirty(button.gameObject);
        }

        Debug.Log($"ButtonSoundUtility: Added UIButtonSound to {addedCount} buttons, skipped {skippedCount} (already had component)");
        
        if (addedCount > 0)
        {
            EditorUtility.DisplayDialog(
                "Batch Add Complete",
                $"Successfully added UIButtonSound to {addedCount} buttons.\nSkipped {skippedCount} buttons that already had the component.",
                "OK"
            );
        }
    }

    [ContextMenu("Remove UIButtonSound from All Buttons")]
    private void RemoveSoundFromAllButtons()
    {
        UIButtonSound[] buttonSounds = FindObjectsOfType<UIButtonSound>(includeInactiveButtons);
        int removedCount = buttonSounds.Length;

        foreach (UIButtonSound buttonSound in buttonSounds)
        {
            DestroyImmediate(buttonSound);
            EditorUtility.SetDirty(buttonSound.gameObject);
        }

        Debug.Log($"ButtonSoundUtility: Removed UIButtonSound from {removedCount} buttons");
        
        if (removedCount > 0)
        {
            EditorUtility.DisplayDialog(
                "Batch Remove Complete",
                $"Successfully removed UIButtonSound from {removedCount} buttons.",
                "OK"
            );
        }
    }

    [ContextMenu("Count Buttons in Scene")]
    private void CountButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(includeInactiveButtons);
        UIButtonSound[] buttonsWithSound = FindObjectsOfType<UIButtonSound>(includeInactiveButtons);

        Debug.Log($"ButtonSoundUtility: Found {allButtons.Length} total buttons, {buttonsWithSound.Length} with UIButtonSound");
        
        EditorUtility.DisplayDialog(
            "Button Count",
            $"Total Buttons: {allButtons.Length}\nWith Sound: {buttonsWithSound.Length}\nWithout Sound: {allButtons.Length - buttonsWithSound.Length}",
            "OK"
        );
    }
#endif
}

#if UNITY_EDITOR
/// <summary>
/// Custom editor for ButtonSoundUtility with easier-to-use buttons
/// </summary>
[CustomEditor(typeof(ButtonSoundUtility))]
public class ButtonSoundUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ButtonSoundUtility utility = (ButtonSoundUtility)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);

        if (GUILayout.Button("Add UIButtonSound to All Buttons", GUILayout.Height(30)))
        {
            utility.SendMessage("AddSoundToAllButtons");
        }

        if (GUILayout.Button("Remove UIButtonSound from All Buttons", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Removal",
                "Are you sure you want to remove UIButtonSound from all buttons?",
                "Yes, Remove",
                "Cancel"))
            {
                utility.SendMessage("RemoveSoundFromAllButtons");
            }
        }

        if (GUILayout.Button("Count Buttons in Scene", GUILayout.Height(30)))
        {
            utility.SendMessage("CountButtons");
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Add this component to any GameObject to use batch operations. " +
            "It will find all buttons in the current scene and add/remove UIButtonSound components.",
            MessageType.Info
        );
    }
}
#endif
