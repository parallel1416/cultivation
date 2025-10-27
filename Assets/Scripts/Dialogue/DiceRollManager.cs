using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Math = System.Math;
using UnityEngine;
using Random = UnityEngine.Random;


public class DiceResult
{
    public int result;
    public string checkDescription;
    public List<int> sizes = new List<int>();
    public List<int> oldResult = new List<int>();
    public List<int> animalResult = new List<int>();
    public List<int> itemResult = new List<int>();

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

    [SerializeField] private string normalDiscipleDesc = "普通弟子";
    [SerializeField] private string jingshiDesc = "经师";
    [SerializeField] private string jianjunDesc = "剑君";
    [SerializeField] private string yuezhengDesc = "乐正";

    [SerializeField] private string mouseDesc = "灵鼠";
    [SerializeField] private string chickenDesc = "凤雏";
    [SerializeField] private string sheepDesc = "獬豸";

    [SerializeField] private string zhiKuiLeiDesc = "纸傀儡";
    [SerializeField] private string yuChanTuiDesc = "玉蝉蜕";
    [SerializeField] private string dianFanTieDesc = "点繁帖";
    [SerializeField] private string wuQueJingDesc = "无缺镜";
    [SerializeField] private string chengFuFuDesc = "承负符";

    [SerializeField] private string reRollDesc = "重投道具";
    [SerializeField] private string enableDesc = "生效!";

    [SerializeField]
    private IReadOnlyList<string> reRollItems = new List<string>()
    {
        "yu_chan_tui",
        "cheng_fu_fu"
    };

    [SerializeField] private bool enableYuezhengSpecialDice = false;
    [SerializeField] private string yuezhengSize = "♪";
    [SerializeField] private string yuezhengHigherNote = "清";
    [SerializeField]
    private IReadOnlyList<int> yuezhengDiceResults = new List<int>()
    {
        1, 2, 3, 5, 6, 8
    };
    public IReadOnlyList<string> yuezhengDiceDisplay = new List<string>()
    {
        "", "宫", "商", "角", "变徵", "徵", "羽", "变宫"
    };


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

    private int GetSizeBasedOnDiceType(string diceType)
    {
        switch (diceType)
        {
            case "Normal":
                return LevelManager.Instance.NormalDiscipleDiceSize;
            case "Jingshi":
                return LevelManager.Instance.JingshiDiceSize;
            case "Jianjun":
                return LevelManager.Instance.JianjunDiceSize;
            case "Yuezheng":
                return LevelManager.Instance.YuezhengDiceSize;
            case "zhi_kui_lei":
                return ItemManager.Instance.PaperPuppetDiceSize;
            default:
                LogController.LogError($"DiceRollManager: Unknown dice type {diceType}");
                return 4; // default to d4
        }
    }

    private string GetDescBasedOnDiceType(string diceType)
    {
        switch (diceType)
        {
            case "Normal":
                return normalDiscipleDesc;
            case "Jingshi":
                return jingshiDesc;
            case "Jianjun":
                return jianjunDesc;
            case "Yuezheng":
                return yuezhengDesc;
            case "zhi_kui_lei":
                return zhiKuiLeiDesc;
            default:
                LogController.LogError($"DiceRollManager: Unknown dice type {diceType}");
                return "";
        }
    }


    /// <summary>
    /// Overload method for simple 1d6 roll, only for debugging, please do not use in game
    /// </summary>
    public DiceResult GetDiceResult()
    {
        int result = Random.Range(1, 5);
        string desc = $"1d4 = {result}";
        return new DiceResult(result, desc);
    }

    /// <summary>
    /// CORE
    /// Generate dice result according to assigned dices, animal and item.
    /// Attach to DialogueManager.Check.cs, method GetCheckResult(), STILL NOT DONE NOW!!!
    /// </summary>
    public DiceResult GetDiceResult(
        Dictionary<string, int> assignedDices, 
        string assignedAnimal, 
        string assignedItem
        )
    {
        DiceResult diceResult = new DiceResult();

        int result = 0; // final result
        StringBuilder desc = new StringBuilder(); // full desc
        StringBuilder finalDesc = new StringBuilder(); // for final line of desc like "1+2+3 = 6 > 5"

        int normalDiceNumber = 0;
        int jingshiAssignStatus = 0;
        int jianjunAssignStatus = 0;
        int yuezhengAssignStatus = 0;
        int zhikuileiAssignStatus = assignedItem == "zhi_kui_lei" ? 1 : 0;
        int totalDiceNumber = 0;

        if (!assignedDices.TryGetValue("Normal", out normalDiceNumber))
        {
            totalDiceNumber += normalDiceNumber;
        }
        if (assignedDices.TryGetValue("Jingshi", out jingshiAssignStatus))
        {
            jingshiAssignStatus = Math.Sign((float)jingshiAssignStatus);
            totalDiceNumber += jingshiAssignStatus;
        }
        if (assignedDices.TryGetValue("Jianjun", out jianjunAssignStatus))
        {
            jianjunAssignStatus = Math.Sign((float)jianjunAssignStatus);
            totalDiceNumber += jianjunAssignStatus;
        }
        if (assignedDices.TryGetValue("Yuezheng", out yuezhengAssignStatus))
        {
            yuezhengAssignStatus = Math.Sign((float)yuezhengAssignStatus);
            totalDiceNumber += yuezhengAssignStatus;
        }
        totalDiceNumber += zhikuileiAssignStatus;

        // Handle Dianfantie item effect: pick a random dice, for set its value to max
        int luckyDice = -1;
        if (assignedItem == "dian_fan_tie")
        {
            luckyDice = Random.Range(0, totalDiceNumber);
        }
        int diceSeqNum = 0;

        // Normal disciple dices
        if (normalDiceNumber > 0)
        {
            for (int i = 0; i < normalDiceNumber; i++)
            {
                RollSingleDice("Normal", diceSeqNum);
                diceSeqNum++;
            }
        }

        // Special disciple dices
        if (jingshiAssignStatus > 0)
        {
            RollSingleDice("Jingshi", diceSeqNum);
            diceSeqNum++;
        }
        if (jianjunAssignStatus > 0)
        {
            RollSingleDice("Jianjun", diceSeqNum);
            diceSeqNum++;
        }
        if (yuezhengAssignStatus > 0)
        {
            RollSingleDice("Yuezheng", diceSeqNum);
            diceSeqNum++;
        }

        // Item dices
        if (assignedItem == "zhi_kui_lei")
        {
            RollSingleDice("zhi_kui_lei", diceSeqNum);
            diceSeqNum++;
        }

        // final result desc


        diceResult.result = result;
        diceResult.checkDescription = desc.ToString();

        return diceResult;



        // Local function for rolling a single dice
        void RollSingleDice(string diceType, int diceSeqNum)
        {
            // Initialization
            int diceSize = GetSizeBasedOnDiceType(diceType);
            string diceDesc = GetDescBasedOnDiceType(diceType);

            bool yuezhengSpecialDice = enableYuezhengSpecialDice && diceType == "Yuezheng";
            bool isHigherMinRoll = assignedItem == "wu_que_jing"; // wuquejing effect: min roll is 2 (never rolls 1)
            bool isReRoll = reRollItems.Contains(assignedItem); // reroll item like yu_chan_tui and cheng_fu_fu
            bool isLucky = assignedItem == "dian_fan_tie" && diceSeqNum == luckyDice; // lucky max item

            int roll;

            // record size
            if (yuezhengSpecialDice)
            {
                diceResult.sizes.Add(diceSize - 1);
            }
            else
            {
                diceResult.sizes.Add(diceSize);
            }

            // desc string of this line ready
            desc.Append($"[ {(diceDesc)}: ] 1d{GetSizeDisplay(diceSize)} = ");


            // First roll (old result)
            roll = GetRoll();
            // record old roll
            diceResult.oldResult.Add(roll);


            // All item logics

            // handle reroll logic, desc including when not reroll
            string reRollResultDesc = "";
            if (isReRoll)
            {
                int roll_new = GetRoll();
                int roll_final = Mathf.Max(roll, roll_new);

                reRollResultDesc = $"({GetRollDisplay(roll)}, {GetRollDisplay(roll_new)}) => {GetRollDisplay(roll_final)} [{reRollDesc}{enableDesc}]";

                roll = roll_final;
            }
            else
            {
                reRollResultDesc = $"{GetRollDisplay(roll)}";

                // handle minroll logic               
                if (isHigherMinRoll)
                {
                    reRollResultDesc += $" [{wuQueJingDesc}{enableDesc}]";
                }
                    
            }
            desc.Append(reRollResultDesc);

            // handle lucky max logic
            string luckyResultDesc = "";
            if (isLucky)
            {
                int roll_max = diceSize;
                roll = roll_max;
                luckyResultDesc = $" => {GetRollDisplay(roll)} [{dianFanTieDesc}{enableDesc}]";
            }
            desc.Append(luckyResultDesc);

            diceResult.itemResult.Add(roll);


            // All animal logics

            // handle plus logic
            if (assignedAnimal == "mouse")
            {
                roll += 1;
                desc.Append($" => {GetRollDisplay(roll)} [{mouseDesc}{enableDesc}]");
            }

            // handle reverse logic
            if (assignedAnimal == "chicken")
            {
                roll = diceSize + 1 - roll;
                desc.Append($" => {GetRollDisplay(roll)} [{chickenDesc}{enableDesc}]");
            }

            // handle 1 to 3 logic
            if (assignedAnimal == "sheep" && roll == 1)
            {
                roll = 3;
                desc.Append($" => {GetRollDisplay(roll)} [{sheepDesc}{enableDesc}]");
            }

            diceResult.animalResult.Add(roll);


            // Final
            result += roll;
            desc.Append('\n');
            finalDesc.Append($"{roll} + ");
            return;



            // local functions to handle special dice display, in a local function (yes)
            int GetRoll()
            {
                int startCorrection = isHigherMinRoll ? 1 : 0; // if assigned wuquejing, roll will never be 1
                if (yuezhengSpecialDice)
                {
                    return yuezhengDiceResults[Random.Range(startCorrection, yuezhengDiceResults.Count)];
                }
                return Random.Range(1 + startCorrection, diceSize + 1);
            }
            string GetRollDisplay(int rollNum)
            {
                if (yuezhengSpecialDice)
                {
                    string rollStr = "?"; // default, for example when rollNum < 0, which might never happen
                    if (rollNum > 0)
                    {
                        rollStr = yuezhengDiceDisplay[(rollNum - 1) % 7 + 1]; // will never overflow in theory
                        if (rollNum > 7)
                        {
                            rollStr = yuezhengHigherNote + rollStr;
                        }
                    }
                    return rollStr;
                }
                return rollNum.ToString();
            }
            string GetSizeDisplay(int sizeNum)
            {
                if (yuezhengSpecialDice) return yuezhengSize;
                return sizeNum.ToString();
            }
        }
    }
}
