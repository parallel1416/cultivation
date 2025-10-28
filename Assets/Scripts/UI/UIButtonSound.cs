using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Add this component to any Button to automatically play click sounds.
/// Can be attached in the Unity Inspector or added via script.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("Sound Settings")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private bool playHoverSound = false;
    [SerializeField] private AudioClip customClickSound; // Optional custom sound
    [SerializeField] private AudioClip customHoverSound; // Optional custom sound

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (!playClickSound || SoundManager.Instance == null)
        {
            return;
        }

        if (customClickSound != null)
        {
            SoundManager.Instance.PlaySFX(customClickSound);
        }
        else
        {
            SoundManager.Instance.PlayButtonClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound || SoundManager.Instance == null)
        {
            return;
        }

        if (customHoverSound != null)
        {
            SoundManager.Instance.PlaySFX(customHoverSound);
        }
        else
        {
            SoundManager.Instance.PlayButtonHover();
        }
    }
}
