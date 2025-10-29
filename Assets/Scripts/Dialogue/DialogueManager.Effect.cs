using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DialogueEffect
{
    public bool showOutput = true;
    public string type = ""; // "money", "disciple", "globalTag", "item"
    public int intValue = 0; // int value for money, item and disciple, tagID for globalTag
    public string stringValue = ""; // tagID for globalTag and itemID for item
    public string operation = ""; // "+"(also works for activate a tag), "-"(also works for deactivate a tag)
}

public partial class DialogueManager : MonoBehaviour
{
    private void ExecuteSentenceEffects(DialogueSentence sentence)
    {
        if (sentence.effects == null || sentence.effects.Count == 0)
            return;

        foreach (var effect in sentence.effects)
        {
            switch (effect.type)
            {
                case "money":
                    HandleMoneyEffect(effect);
                    break;

                case "disciple":
                    HandleDiscipleEffect(effect);
                    break;

                case "globalTag":
                    HandleGlobalTagEffect(effect);
                    break;

                default:
                    LogController.LogWarning($"Unknown effect type: {effect.type}");
                    break;
            }
        }
    }

    /// <summary>
    /// Handle money effects (add/subtract)
    /// </summary>
    private void HandleMoneyEffect(DialogueEffect effect)
    {
        int amount = effect.intValue;
        switch (effect.operation)
        {
            case "+":
                LevelManager.Instance.AddMoney(amount);
                break;

            case "-":
                LevelManager.Instance.SpendMoney(amount);
                break;

            default:
                LogController.LogError($"Invalid operation: {effect.operation}");
                return;
        }
        LogController.Log($"Effect: {effect.operation} {amount} money");
    }

    /// <summary>
    /// Handle disciple effects (plus/minus)
    /// </summary>
    private void HandleDiscipleEffect(DialogueEffect effect)
    {
        int amount = effect.intValue;
        switch (effect.operation)
        {
            case "+":
                LevelManager.Instance.AddDisciples(amount);
                break;

            case "-":
                LevelManager.Instance.DismissDisciples(amount);
                break;

            default:
                LogController.LogError($"Invalid operation: {effect.operation}");
                return;
        }
        LogController.Log($"Effect: {effect.operation} {amount} disciple(s)");
    }

    /// <summary>
    /// Handle global tag effects (+/-)
    /// </summary>
    private void HandleGlobalTagEffect(DialogueEffect effect)
    {
        if (!GlobalTagManager.Instance.HasTag(effect.stringValue))
        {
            LogController.LogError($"GlobalTag not exist, ID: {effect.stringValue}");
            return;
        }
        switch (effect.operation)
        {
            case "+":
                GlobalTagManager.Instance.EnableTag(effect.stringValue);
                break;

            case "-":
                GlobalTagManager.Instance.DisableTag(effect.stringValue);
                break;

            default:
                LogController.LogError($"Invalid operation: {effect.operation}");
                return;
        }
        string tagEffectLogOutput = effect.operation == "+" ? "enabled" : "disabled";
        LogController.Log($"GlobalTag {tagEffectLogOutput}, ID: {effect.stringValue}");
    }
}
