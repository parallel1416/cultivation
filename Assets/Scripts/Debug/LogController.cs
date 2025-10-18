using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toggle for enabling/disabling debug logs.
/// IMPORTANT: Use LogController.Log() and LogController.LogError() instead of Debug.Log() and Debug.LogError()
/// </summary>

public class LogController : MonoBehaviour
{
    [SerializeField] private bool enableDebugLog = false;

    private static LogController _instance;

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

    public static void Log(string message)
    {
        if (_instance != null && _instance.enableDebugLog)
        {
            Debug.Log(message);
        }
    }

    public static void LogError(string message)
    {
        if (_instance != null && _instance.enableDebugLog)
        {
            Debug.LogError(message);
        }
    }
}
