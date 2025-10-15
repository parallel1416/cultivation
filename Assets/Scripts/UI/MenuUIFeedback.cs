using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides visual feedback for the circular menu
/// Shows instructions and current selection
/// </summary>
public class MenuUIFeedback : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text instructionText;
    [SerializeField] private Text selectionLabel;
    [SerializeField] private Image selectionIcon;
    [SerializeField] private GameObject scrollHint;

    [Header("Messages")]
    [SerializeField] private string defaultInstruction = "Scroll to browse • Click to select";
    [SerializeField] private bool hideScrollHintAfterUse = true;

    private bool hasScrolled = false;

    private void Start()
    {
        if (instructionText != null)
            instructionText.text = defaultInstruction;

        if (scrollHint != null)
            scrollHint.SetActive(true);
    }

    private void Update()
    {
        // Detect first scroll and hide hint
        if (!hasScrolled && hideScrollHintAfterUse)
        {
            if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                hasScrolled = true;
                if (scrollHint != null)
                {
                    StartCoroutine(FadeOutScrollHint());
                }
            }
        }
    }

    public void UpdateSelection(string optionName, Sprite optionIcon)
    {
        if (selectionLabel != null)
            selectionLabel.text = optionName;

        if (selectionIcon != null && optionIcon != null)
            selectionIcon.sprite = optionIcon;
    }

    public void ShowInstruction(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }

    public void ResetInstruction()
    {
        if (instructionText != null)
            instructionText.text = defaultInstruction;
    }

    private System.Collections.IEnumerator FadeOutScrollHint()
    {
        CanvasGroup cg = scrollHint.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = scrollHint.AddComponent<CanvasGroup>();

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        scrollHint.SetActive(false);
    }
}
