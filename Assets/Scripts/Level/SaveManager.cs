using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


/// <summary>
/// A save will be created after a turn ends.
/// Actions during a turn will not be recorded until player clicks NextTurnButton and the turn ends.
/// There will be only 1 save. Maybe can be expanded in the future.
/// Re-entering the game will try to load the (only) save.
/// If there is no save, the game will try to initialize.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance => _instance;

    [SerializeField] private bool enableSave = true;
    [SerializeField] private string saveFileDirectory = Application.persistentDataPath + "/Saves/";
    [SerializeField] private string saveFileFormat = ".json";
    [SerializeField] private bool singleSave = true;

    private SaveData saveData; // the only save

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }


    /// <summary>
    /// Create a new save and overlay it on the existing save.
    /// Store the save data to a file. Will also overlay the existing file.
    /// </summary>
    public void CreateSave()
    {
        if (!enableSave) return;

        saveData = CollectSaveData(); // collect all game info from game
        string jsonData = JsonUtility.ToJson(saveData, true);

        DateTime time = DateTime.Now;
        int turn = saveData.turn;
        if (singleSave)
        {
            string fileName = GenerateSaveName_Single();
            string fullPath = saveFileDirectory + fileName;

            try // create save file
            {
                File.Delete(fullPath);
                File.WriteAllText(fullPath, jsonData);
            }
            catch (Exception e)
            {
                LogController.LogError($"SaveManager: Fail to create save due to: {e.Message}");
            }
        }
        else
        {
            string fileName = GenerateSaveName_Multi(turn, time);
            // multisave logic
        }       
    }

    /// <summary>
    /// Delete the only save (just the saveData object, not the file).
    /// Will only be triggered if a save exists and the player clicks "Start (a new game)" in the main menu.
    /// </summary>
    public void DestroySave()
    {
        saveData = null;
    }

    /// <summary>
    /// Load the only save from file
    /// </summary>
    public void LoadSave()
    {
        try
        {
            if (singleSave)
            {
                string filePath = GenerateSaveName_Single();
                if (!File.Exists(filePath))
                {
                    LogController.Log("SaveManager: No save file in file directory! Will start a new game.");
                    return;
                }

                string jsonData = File.ReadAllText(filePath);
                saveData = JsonUtility.FromJson<SaveData>(jsonData);
                ApplySaveData();
            }
            else
            {
                //multisave logic
            }
        }
        catch (Exception e)
        {
            LogController.LogError($"SaveManager: Fail to load save due to: {e.Message}");
        }
    }

    /// <summary>
    /// Collect current game data into a SaveData object
    /// </summary>
    public SaveData CollectSaveData()
    {
        SaveData saveData = new SaveData();

        saveData.money = LevelManager.Instance.Money;
        saveData.disciples = LevelManager.Instance.Disciples;

        saveData.turn = TurnManager.Instance.CurrentTurn;

        saveData.statusMouse = LevelManager.Instance.StatusMouse;
        saveData.statusChicken = LevelManager.Instance.StatusChicken;
        saveData.statusSheep = LevelManager.Instance.StatusSheep;

        saveData.statusJingshi = LevelManager.Instance.StatusJingshi;
        saveData.statusJianjun = LevelManager.Instance.StatusJianjun;
        saveData.statusYuezheng = LevelManager.Instance.StatusYuezheng;

        saveData.tagMap = GlobalTagManager.Instance.TagMap;
        saveData.techNodes = TechManager.Instance.TechNodes;

        saveData.items = ItemManager.Instance.Items;

        return saveData;
    }

    /// <summary>
    /// IMPORTANT!
    /// Public method to check if a save exists, for example deciding whether to show "Continue" button in main menu
    /// </summary>
    public bool HasSave() => File.Exists(GenerateSaveName_Single());

    /// <summary>
    /// Apply data from 
    /// </summary>
    public void ApplySaveData()
    {
        if (saveData == null) return;
        TurnManager.Instance.ApplySaveData(saveData);
        LevelManager.Instance.ApplySaveData(saveData);
        GlobalTagManager.Instance.ApplySaveData(saveData);
        TechManager.Instance.ApplySaveData(saveData);
        ItemManager.Instance.ApplySaveData(saveData);
    }

    private string GenerateSaveName_Single()
    {
        StringBuilder saveName = new StringBuilder();
        saveName.Append("Save");
        saveName.Append(saveFileFormat);
        return saveName.ToString();
    }   

    private string GenerateSaveName_Multi(int turn, DateTime time)
    {
        StringBuilder saveName = new StringBuilder();
        saveName.Append("Save_Turn_");
        saveName.Append(turn);
        saveName.Append("_");
        saveName.Append(time.ToString("yyyyMMdd_HHmmss"));
        saveName.Append(saveFileFormat);
        return saveName.ToString();
    }
}
