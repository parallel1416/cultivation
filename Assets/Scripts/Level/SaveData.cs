using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SaveData : MonoBehaviour
{
    // Save info
    public string savename = "";
    public long timestamp = 0;

    // Turn info
    public int turn;

    // Level info
    public int money;
    public int disciples;

    public int statusMouse;
    public int statusChicken;
    public int statusSheep;

    public int statusJingshi;
    public int statusJianjun;
    public int statusYuezheng;

    // Global tag info
    public Dictionary<string, GlobalTag> tagMap;

    // Tech info
    public Dictionary<string, TechNode> techNodes;

    // Item info
}
