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

    [SerializeField] private readonly string normalDiscipleDesc = "普通门徒";
    [SerializeField] private readonly string jingshiDesc = "经师";
    [SerializeField] private readonly string jianjunDesc = "剑君";
    [SerializeField] private readonly string yuezhengDesc = "乐正";

    [SerializeField] private readonly string mouseDesc = "灵鼠";
    [SerializeField] private readonly string chickenDesc = "凤雏";
    [SerializeField] private readonly string sheepDesc = "獬豸";

    [SerializeField] private readonly string zhiKuiLeiDesc = "纸傀儡";
    [SerializeField] private readonly string yuChanTuiDesc = "玉蝉蜕";
    [SerializeField] private readonly string dianFanTieDesc = "点繁帖";
    [SerializeField] private readonly string wuQueJingDesc = "无缺镜";
    [SerializeField] private readonly string chengFuFuDesc = "承负符";
    [SerializeField] private readonly string jianPuCanZhangDesc = "剑谱残章";
    [SerializeField] private readonly string feiGuangJianFuDesc = "飞光剑符";

    [SerializeField] private readonly string reRollDesc = "重投道具";
    [SerializeField] private readonly string enableDesc = "生效!";
    [SerializeField] private readonly string becauseDesc = "因";
    [SerializeField] private readonly string minusDesc = "而减少！";
    [SerializeField] private readonly string plusDesc = "而增加！";

    [SerializeField] private readonly string finalDesc = "最终结果：";

    [SerializeField]
    private IReadOnlyList<string> reRollItems = new List<string>()
    {
        "yu_chan_tui",
        "cheng_fu_fu"
    };

    [SerializeField] private readonly bool enableYuezhengSpecialDice = true;
    [SerializeField] private readonly string yuezhengSize = "♪";
    [SerializeField] private readonly string yuezhengHigherNote = "清";
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

        // Single dice rolls

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

        // Item effects
        if (assignedItem == "cheng_fu_fu" && result != 0)
        {
            result -= 2;
            finalDesc.Append($" - 2({chengFuFuDesc})");
        }

        if (assignedItem == "fei_guang_jian_fu")
        {
            result += 10;
            finalDesc.Append($" + 10({feiGuangJianFuDesc})");
        }

        if (finalDesc.Length == 0) // no dice or plus item assigned
        {
            finalDesc.Append(finalDesc);
            finalDesc.Append(result);
        }
        else
        {
            finalDesc.Remove(0, 3); // remove first " + " or " - "
            finalDesc.Insert(0, finalDesc);
            finalDesc.Append($" = {result}");
        }

        // final result desc

        desc.AppendLine(finalDesc.ToString());
        diceResult.result = result;
        diceResult.checkDescription = desc.ToString().TrimEnd();

        return diceResult;



        // Local function for rolling a single dice
        void RollSingleDice(string diceType, int diceSeqNum)
        {
            // Initialization
            int diceSize = GetSizeBasedOnDiceType(diceType);
            string diceDesc = GetDescBasedOnDiceType(diceType);

            StringBuilder lineDesc = new StringBuilder();

            bool yuezhengSpecialDice = enableYuezhengSpecialDice && diceType == "Yuezheng";
            bool isHigherMinRoll = assignedItem == "wu_que_jing"; // wuquejing effect: min roll is 2 (never rolls 1)
            bool isReRoll_1 = assignedItem == "yu_chan_tui";
            bool isReRoll_2 = assignedItem == "cheng_fu_fu";
            bool isLucky = assignedItem == "dian_fan_tie" && diceSeqNum == luckyDice; // lucky max item
            bool isAverage = assignedItem == "jian_pu_can_zhang";

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
            lineDesc.Append($"[ {(diceDesc)}: ] 1d{GetSizeDisplay(diceSize)} = ");


            // First roll (old result)
            roll = GetRoll();
           


            // All item logics

            // handle reroll logic, desc including when not reroll
            string reRollResultDesc = "";
            if (isReRoll_1) // yuchantui
            {
                int roll_new = GetRoll();
                int roll_final = Mathf.Max(roll, roll_new);
                int roll_old = Mathf.Min(roll, roll_new); // make player happy
                // record old roll
                diceResult.oldResult.Add(roll_old);

                reRollResultDesc = $"({GetRollDisplay(roll)}, {GetRollDisplay(roll_new)}) => {GetRollDisplay(roll_final)} [{yuChanTuiDesc}{enableDesc}]";

                roll = roll_final;
            }
            else if (isReRoll_2) //chengfufu
            {
                int roll_new_1 = GetRoll();
                int roll_new_2 = GetRoll();
                int roll_final = Mathf.Max(roll, roll_new_1, roll_new_2);
                int roll_old = Mathf.Min(roll, roll_new_1, roll_new_2); // make player happy
                // record old roll
                diceResult.oldResult.Add(roll_old);

                reRollResultDesc = $"({GetRollDisplay(roll)}, {GetRollDisplay(roll_new_1)}, {GetRollDisplay(roll_new_2)}) => {GetRollDisplay(roll_final)} [{chengFuFuDesc}{enableDesc}]";

                roll = roll_final;
            }
            else
            {
                // record old roll
                diceResult.oldResult.Add(roll);
                reRollResultDesc = $"{GetRollDisplay(roll)}";

                // handle minroll logic, only desc              
                if (isHigherMinRoll)
                {
                    reRollResultDesc += $" [{wuQueJingDesc}{enableDesc}]";
                }

            }
            lineDesc.Append(reRollResultDesc);

            // handle lucky max logic
            string luckyResultDesc = "";
            if (isLucky)
            {
                int roll_max = diceSize;
                roll = roll_max;
                luckyResultDesc = $" => {GetRollDisplay(roll)} [{dianFanTieDesc}{enableDesc}]";
            }
            lineDesc.Append(luckyResultDesc);

            // handle average logic
            string averageResultDesc = "";
            if (isAverage)
            {
                int averageRoll = diceSize / 2 + 1; // d4 to 3, d6 to 4, d8 to 5, etc.
                roll = averageRoll;
                averageResultDesc = $" => {GetRollDisplay(roll)} [{jianPuCanZhangDesc}{enableDesc}]";
            }
            lineDesc.Append(averageResultDesc);

            diceResult.itemResult.Add(roll);




            // All animal logics

            // handle plus logic
            if (assignedAnimal == "mouse")
            {
                roll += 1;
                lineDesc.Append($" => {GetRollDisplay(roll)} [{mouseDesc}{enableDesc}]");
            }

            // handle reverse logic
            if (assignedAnimal == "chicken")
            {
                roll = diceSize + 1 - roll;
                lineDesc.Append($" => {GetRollDisplay(roll)} [{chickenDesc}{enableDesc}]");
            }

            // handle 1 to 3 logic
            if (assignedAnimal == "sheep" && roll == 1)
            {
                roll = 3;
                lineDesc.Append($" => {GetRollDisplay(roll)} [{sheepDesc}{enableDesc}]");
            }

            diceResult.animalResult.Add(roll);


            // Final
            result += roll;
            desc.AppendLine(lineDesc.ToString());
            finalDesc.Append($" + {roll}");
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
