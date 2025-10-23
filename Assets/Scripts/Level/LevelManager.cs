using Unity.VisualScripting;
using UnityEngine;

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

    public int Money => money;
    // public int MoneyPerTurn => moneyPerTurn;
    public int Disciples => disciples;
    //public int DisciplesPerTurn => disciplesPerTurn;

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

    /// <summary>
    /// Resource management methods: AddMoney/AddDisciples increase resources, SpendMoney/SpendDisciples decrease them if sufficient.
    /// </summary>

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("Cannot add negative money amount.");
            return;
        }
        money += amount;
        LogController.Log($"{amount}money added!");
    }

    public bool SpendMoney(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("Cannot spend negative money amount.");
            return false;
        }
        if (money >= amount)
        {
            money -= amount;
            LogController.Log($"{amount}money spent!");
            return true;
        }
        LogController.LogError("Not enough money to spend.");
        return false;
    }

    // If the player doesnt have enough money, it will spend all money and set money to 0.
    public void ForceSpendMoney(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("Cannot force spend negative money amount.");
            return;
        }
        if (money >= amount)
        {
            money -= amount;
            LogController.Log($"{amount}money force spent!");
            return;
        }
        LogController.Log("Not enough money to force spend! Set money to 0.");
        return;
    }

    public void AddDisciples(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("Cannot add negative disciple amount.");
            return;
        }
        disciples += amount;
        LogController.Log($"{amount}disciples added!");
    }

    public bool SpendDisciples(int amount)
    {
        if (amount < 0)
        {
            LogController.LogError("Cannot spend negative disciple amount.");
            return false;
        }
        if (disciples >= amount)
        {
            disciples -= amount;
            LogController.Log($"{amount}disciples spent!");
            return true;
        }
        LogController.LogError("Not enough disciples to spend.");
        return false;
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