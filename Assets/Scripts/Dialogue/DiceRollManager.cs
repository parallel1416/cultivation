using System.Collections.Generic;
using System.Text;
using UnityEngine;


public class DiceResult
{
    public int result;
    public string checkDescription;

    public DiceResult()
    {
        this.result = 0;
        this.checkDescription = "";
    }
    public DiceResult(int result, string checkDescription)
    {
        this.result = result;
        this.checkDescription = checkDescription;
    }
}

public class DiceRollManager : MonoBehaviour
{
    private static DiceRollManager _instance;
    public static DiceRollManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public DiceResult GetDiceResult()
    {
        int result = Random.Range(1, 6);
        string desc = $"1d6 = {result}";
        return new DiceResult(result, desc);
    }



    public DiceResult GetDiceResult(Dictionary<string, int> AssignedDices)
    {
        int result = 0;
        StringBuilder desc = new StringBuilder();

        if (AssignedDices.TryGetValue("Normal", out int normalDices))
        {
            for (int i = 0; i < normalDices; i++)
            {
                int diceScale = LevelManager.Instance.NormalDiscipleDiceScale;
                int roll = Random.Range(1, diceScale);
                result += roll;
                desc.Append($"1d{diceScale} = {roll} + ");
            }
        }
        else LogController.LogError("DiceRollManager: AssignedDice dictionary error! No normal disciple dice kvp.");

        if (AssignedDices.TryGetValue("Jingshi", out int JingshiAssigned))
        {
            if (JingshiAssigned > 0)
            {
                int diceScale = LevelManager.Instance.JingshiDiceScale;
                int roll = Random.Range(1, diceScale);
                result += roll;
                desc.Append($"1d{diceScale} = {roll} + ");
            }
        }
        else LogController.Log("DiceRollManager: No Jingshi dice kvp, maybe not assigned.");

        if (AssignedDices.TryGetValue("Jianjun", out int JianjunAssigned))
        {
            if (JianjunAssigned > 0)
            {
                int diceScale = LevelManager.Instance.JianjunDiceScale;
                int roll = Random.Range(1, diceScale);
                result += roll;
                desc.Append($"1d{diceScale} = {roll} + ");
            }
        }
        else LogController.Log("DiceRollManager: No Jianjun dice kvp, maybe not assigned.");

        if (AssignedDices.TryGetValue("Yuezheng", out int YuezhengAssigned))
        {
            if (YuezhengAssigned > 0)
            {
                int diceScale = LevelManager.Instance.YuezhengDiceScale;
                int roll = Random.Range(1, diceScale);
                result += roll;
                desc.Append($"1d{diceScale} = {roll} + ");
            }
        }
        else LogController.Log("DiceRollManager: No Yuezheng dice kvp, maybe not assigned.");

        if (desc.Length >= 3) desc.Replace(" + ", " = ", desc.Length - 3, 3); // end of the desc string

        return new DiceResult(result, desc.ToString());
    }
}
