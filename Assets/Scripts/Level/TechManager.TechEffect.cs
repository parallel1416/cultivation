using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TechManager: MonoBehaviour
{
    [SerializeField] private int techDisciplesNum_Recruit_1 = 5;
    [SerializeField] private int techDisciplesNum_Recruit_2 = 5;
    [SerializeField] private int techDisciplesNum_Recruit_3 = 5;

    public void ApplyUnlockTechEffect(TechNode techNode)
    {
        string techId = techNode.id;

        // Example effect application based on tech ID
        switch (techId)
        {
            case "recruit_normal_1":
                LevelManager.Instance.AddDisciples(techDisciplesNum_Recruit_1);
                break;

            case "recruit_normal_2":
                LevelManager.Instance.AddDisciples(techDisciplesNum_Recruit_2);
                break;

            case "recruit_normal_3":
                LevelManager.Instance.AddDisciples(techDisciplesNum_Recruit_3);
                break;

            case "mouse":
            case "pet_1":
                LevelManager.Instance.StatusMouse = 1;
                DialogueManager.PlayDialogueEvent("mouse");
                break;

            case "sheep":
            case "pet_2":
                LevelManager.Instance.StatusSheep = 1;
                DialogueManager.PlayDialogueEvent("hen_0");
                break;

            case "chicken":
            case "hen":
            case "pet_3":
                LevelManager.Instance.StatusChicken = 1;
                DialogueManager.PlayDialogueEvent("sheep_0");
                break;

            case "recruit":
                DialogueManager.PlayDialogueEvent("jingshi_0");
                break;

            case "recruit_special_1":
                LevelManager.Instance.StatusJianjun = 1;
                DialogueManager.PlayDialogueEvent("jianjun_0");
                break;

            case "recruit_special_3":
                LevelManager.Instance.StatusYuezheng = 1;
                DialogueManager.PlayDialogueEvent("yuezheng_0");
                break;

            case "story_3":
                DialogueManager.PlayDialogueEvent("sacrifice");
                break;

            default:
                Debug.Log($"TechManager: Tech exists, but no immediate effect defined for tech ID: {techId}");
                break;
        }
    }

    // Overload to apply effect by tech ID (quick access)
    public void ApplyUnlockTechEffect(string techId)
    {
        if (techNodes.TryGetValue(techId, out TechNode techNode))
        {
            ApplyUnlockTechEffect(techNode);
        }
        else
        {
            Debug.LogError($"TechManager: Tech ID {techId} not found in tech nodes, can't apply unlock effects.");
        }
    }

    public void ApplyDismantleTechEffect(TechNode techNode)
    {
        string techId = techNode.id;

        // Example effect removal based on tech ID
        switch (techId)
        {
            case "recruit_normal_1":
                LevelManager.Instance.DismissDisciples(techDisciplesNum_Recruit_1);
                break;

            case "recruit_normal_2":
                LevelManager.Instance.DismissDisciples(techDisciplesNum_Recruit_2);
                DialogueManager.PlayDialogueEvent("dismiss_-1");
                break;

            case "recruit_normal_3":
                LevelManager.Instance.DismissDisciples(techDisciplesNum_Recruit_3);
                DialogueManager.PlayDialogueEvent("dismiss_-1");
                break;

            case "mouse":
            case "pet_1":
                LevelManager.Instance.StatusMouse = -1;
                DialogueManager.PlayDialogueEvent("mouse_-1");
                break;

            case "sheep":
            case "pet_2":
                LevelManager.Instance.StatusSheep = -1;
                DialogueManager.PlayDialogueEvent("sheep_-1");
                break;

            case "chicken":
            case "hen":
            case "pet_3":
                LevelManager.Instance.StatusChicken = -1;
                DialogueManager.PlayDialogueEvent("hen_-1");
                break;

            case "farm_1":
                DialogueManager.PlayDialogueEvent("dismantle_-1");
                break;

            case "farm_2":
                DialogueManager.PlayDialogueEvent("dismantle_-2");
                break;

            default:
                Debug.Log($"TechManager: Tech exists, but no immediate dismantle effect defined for tech ID: {techId}");
                break;
        }
    }

    // Overload to apply effect by tech ID (quick access)
    public void ApplyDismantleTechEffect(string techId)
    {
        if (techNodes.TryGetValue(techId, out TechNode techNode))
        {
            ApplyDismantleTechEffect(techNode);
        }
        else
        {
            Debug.LogError($"TechManager: Tech ID {techId} not found in tech nodes, can't apply dismantle effects.");
        }
    }
}
