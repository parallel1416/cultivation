using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages level resources like money and disciples
/// </summary>

public class LevelManager : MonoBehaviour
{
    // Singleton instance
    private static LevelManager _instance;
    public static LevelManager Instance => _instance;

    // Core resources
    // SerializeField only for debugging in inspector
    [SerializeField] private int money = 200;
    [SerializeField] private int disciples = 10;
    //[SerializeField] private int moneyPerTurn = 0;
    //[SerializeField] private int disciplesPerTurn = 0;

    // status
    // normal disciples
    [SerializeField] private int activeDisciples = 0;

    // spirit animals
    [SerializeField] private int statusMouse = -1;
    [SerializeField] private int statusChicken = -1;
    [SerializeField] private int statusSheep = -1;

    // special disciples
    [SerializeField] private int statusJingshi = -1;
    [SerializeField] private int statusJianjun = -1;
    [SerializeField] private int statusYuezheng = -1;

    // dice control
    [SerializeField] private int normalDiscipleDiceSize = 4;
    [SerializeField] private int jingshiDiceSize = 8;
    [SerializeField] private int jianjunDiceSize = 8;
    [SerializeField] private int yuezhengDiceSize = 8;

    public int Money => money;
    // public int MoneyPerTurn => moneyPerTurn;
    public int Disciples => disciples;
    //public int DisciplesPerTurn => disciplesPerTurn;

    public int ActiveDisciples
    {
        get => activeDisciples;
        set // Well, maybe a useless setter.
        {
            if (value > disciples)
            {
                activeDisciples = value;
                LogController.Log($"LevelManager: Too much active disciples({value})! Set to max({disciples}).");
            }
            else if (value < 0)
            {
                activeDisciples = 0;
                LogController.Log($"LevelManager: Negative number of active disciples({value})! Set to 0.");
            }
            else activeDisciples = value;
        }
    }
    public int StatusMouse
    { get => statusMouse; set => statusMouse = Math.Sign(value); }

    public int StatusChicken
    { get => statusChicken; set => statusChicken = Math.Sign(value); }

    public int StatusSheep
    { get => statusSheep; set => statusSheep = Math.Sign(value); }

    public int StatusJingshi
    { get => statusJingshi; set => statusJingshi = Math.Sign(value); }

    public int StatusJianjun
    { get => statusJianjun; set => statusJianjun = Math.Sign(value); }

    public int StatusYuezheng
    { get => statusYuezheng; set => statusYuezheng = Math.Sign(value); }

    public int NormalDiscipleDiceSize => normalDiscipleDiceSize;
    public int JingshiDiceSize => jingshiDiceSize;
    public int JianjunDiceSize => jianjunDiceSize;
    public int YuezhengDiceSize => yuezhengDiceSize;

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

    public void StartNewGame()
    {

    }


    /// <summary>
    /// Resource management methods: AddMoney/AddDisciples increase resources, SpendMoney/SpendDisciples decrease them if sufficient.
    /// </summary>

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("LevelManager: Cannot add negative money amount.");
            return;
        }
        money += amount;
        LogController.Log($"LevelManager: {amount} money added!");
    }

    public bool SpendMoney(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("LevelManager: Cannot spend negative money amount.");
            return false;
        }
        if (money >= amount)
        {
            money -= amount;
            LogController.Log($"LevelManager: {amount} money spent!");
            return true;
        }
        LogController.LogError("LevelManager: Not enough money to spend.");
        return false;
    }

    // If the player doesnt have enough money, it will spend all money and set money to 0.
    public void ForceSpendMoney(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("LevelManager: Cannot force spend negative money amount.");
            return;
        }
        if (money >= amount) // same as normal spend
        {
            money -= amount;
            LogController.Log($"LevelManager: {amount} money force spent!");
            return;
        }
        money = 0;
        LogController.Log("LevelManager: Not enough money to force spend! Set money to 0.");
        return;
    }

    public void AddDisciples(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("LevelManager: Cannot add negative disciple amount.");
            return;
        }
        disciples += amount;
        LogController.Log($"LevelManager: {amount} disciples added!");
    }

    /// <summary>
    /// This will reduce the upper limit of available disciples per turn!
    /// Will force kill to zero if an overkill happens.
    /// </summary>
    public bool KillDisciples(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("LevelManager: Cannot kill negative disciple amount.");
            return false;
        }
        if (disciples >= amount) // success
        {
            disciples -= amount;
            LogController.Log($"LevelManager: {amount} disciples killed!");
            return true;
        }
        disciples = 0;
        LogController.LogError("LevelManager: Not enough disciples to kill, set to 0.");
        return false;
    }

    /// <summary>
    /// Only to reduce active disciple number! for example, assign to an event
    /// Need accompanying UI refresh after this method
    ///// </summary>
    public bool SpendActiveDisciples(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("LevelManager: Cannot spend negative disciple amount.");
            return false;
        }
        if (activeDisciples >= amount) // success
        {
            activeDisciples -= amount;
            LogController.Log($"LevelManager: {amount} disciples set to inactive!");
            return true;
        }
        LogController.LogError("LevelManager: Not enough disciples to spend.");
        return false;
    }

    public void ResetActiveDisciples()
    {
        activeDisciples = disciples;
    }

    public Dictionary<string, int> GetAllStatus()
    {
        return new Dictionary<string, int>
        {
            { "Normal", ActiveDisciples },
            { "Mouse", StatusMouse },
            { "Chicken", StatusChicken },
            { "Sheep", StatusSheep },
            { "Jingshi", StatusJingshi },
            { "Jianjun", StatusJianjun },
            { "Yuezheng", StatusYuezheng }
        };
    }

    /// <summary>
    /// Apply data from SavaManager to game state
    /// </summary>
    public void ApplySaveData(SaveData saveData)
    {
        money = saveData.money;
        disciples = saveData.disciples;

        statusChicken = saveData.statusChicken;
        statusMouse = saveData.statusMouse;
        statusSheep = saveData.statusSheep;

        statusJianjun = saveData.statusJianjun;
        statusJingshi = saveData.statusJingshi;
        statusYuezheng = saveData.statusYuezheng;
    }

    /// <summary>
    /// Turn-based resource generation
    /// temporarily deprecated
    /// </summary>

    //public void GenerateResourcesPerTurn()
    //{
    //    AddMoney(moneyPerTurn);
    //    AddDisciples(disciplesPerTurn);
    //    LogController.Log($"Generated resources for the turn: {moneyPerTurn} money and {disciplesPerTurn} disciples.");
    //}
}