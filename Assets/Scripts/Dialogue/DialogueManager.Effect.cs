using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DialogueManager : MonoBehaviour
{
    private void ExecuteSentenceEffects(DialogueSentence sentence)
    {
        if (sentence.effects == null || sentence.effects.Count == 0)
            return;

        foreach (var effect in sentence.effects)
        {
            switch (effect.effectType)
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
                    LogController.LogWarning($"Unknown effect type: {effect.effectType}");
                    break;
            }
        }
    }

    /// <summary>
    /// Handle money effects (add/subtract)
    /// </summary>
    private void HandleMoneyEffect(DialogueEffect effect)
    {
        if (int.TryParse(effect.value, out int amount))
        {
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
        }
        else
        {
            LogController.LogError($"Invalid money value: {effect.value}");
        }
    }

    /// <summary>
    /// Handle disciple effects (plus/minus)
    /// </summary>
    private void HandleDiscipleEffect(DialogueEffect effect)
    {
        if (int.TryParse(effect.value, out int amount))
        {
            switch (effect.operation)
            {
                case "+":
                    LevelManager.Instance.AddDisciples(amount);
                    break;

                case "-":
                    LevelManager.Instance.SpendDisciples(amount);
                    break;

                default:
                    LogController.LogError($"Invalid operation: {effect.operation}");
                    return;
            }
            LogController.Log($"Disciple effect: {effect.operation} {amount} disciples");
        }
        else
        {
            LogController.LogError($"Invalid disciple value: {effect.value}");
        }
    }

    /// <summary>
    /// Handle global tag effects (+/-)
    /// </summary>
    private void HandleGlobalTagEffect(DialogueEffect effect)
    {
        if (!GlobalTagManager.Instance.HasTag(effect.value))
        {
            return;
        }
        switch (effect.operation)
        {
            case "+":
                GlobalTagManager.Instance.EnableTag(effect.value);
                break;

            case "-":
                GlobalTagManager.Instance.DisableTag(effect.value);
                break;

            default:
                LogController.LogError($"Invalid operation: {effect.operation}");
                return;
        }
    }
}
