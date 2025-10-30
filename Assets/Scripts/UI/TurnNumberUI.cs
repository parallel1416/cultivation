using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnNumberUI : MonoBehaviour
{
    [Header("Display Bind")]
    [SerializeField] private TextMeshProUGUI displayText;

    private void Start()
    {
        if (displayText == null)
        {
            displayText = GetComponent<TextMeshProUGUI>();
        }
        RefreshDisplay();

        if (TurnManager.Instance != null)
        {
            TurnManager.OnTurnChanged += RefreshDisplay;
        }
    }

    private void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.OnTurnChanged -= RefreshDisplay;
        }
    }

    public void RefreshDisplay()
    {
        if (displayText != null && TurnManager.Instance != null)
        {
            displayText.text = TurnManager.Instance.CurrentTurn.ToString();
        }
    }
}
