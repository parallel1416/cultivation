using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TechManager: MonoBehaviour
{
    [SerializeField] private int techDisciplesNum_Recruit_1 = 5;
    [SerializeField] private int techDisciplesNum_Recruit_2 = 5;
    [SerializeField] private int techDisciplesNum_Recruit_3 = 5;
    public void ApplyUnlockTechEffect(string techId)
    {
        if (techNodes.TryGetValue(techId, out TechNode techNode))
        {
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

                default:
                    Debug.Log($"TechManager: Tech exists, but no immediate effect defined for tech ID: {techId}");
                    break;
            }
        }
        else
        {
            Debug.LogError($"TechManager: Tech ID {techId} not found in tech nodes, can't apply unlock effects.");
        }
    }

    public void ApplyDismantleTechEffect(string techId)
    {
        if (techNodes.TryGetValue(techId, out TechNode techNode))
        {
            // Example effect removal based on tech ID
            switch (techId)
            {
                case "recruit_normal_1":
                    LevelManager.Instance.KillDisciples(techDisciplesNum_Recruit_1);
                    break;

                case "recruit_normal_2":
                    LevelManager.Instance.KillDisciples(techDisciplesNum_Recruit_2);
                    break;

                case "recruit_normal_3":
                    LevelManager.Instance.KillDisciples(techDisciplesNum_Recruit_3);
                    break;

                default:
                    Debug.Log($"TechManager: Tech exists, but no immediate dismantle effect defined for tech ID: {techId}");
                    break;
            }
        }
        else
        {
            Debug.LogError($"TechManager: Tech ID {techId} not found in tech nodes, can't apply dismantle effects.");
        }
    }
}
