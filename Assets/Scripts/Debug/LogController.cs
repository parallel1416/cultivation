using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toggle for enabling/disabling debug logs.
/// IMPORTANT: Use LogController.Log() and LogController.LogError() instead of Debug.Log() and Debug.LogError()
/// </summary>

public class LogController : MonoBehaviour
{
    [SerializeField] private static bool enableDebugLog = true;

    public static void Log(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log(message);
        }
    }

    public static void LogWarning(string message)
    {
        if (enableDebugLog)
        {
            Debug.LogWarning(message);
        }
    }

    public static void LogError(string message)
    {
        if (enableDebugLog)
        {
            Debug.LogError(message);
        }
    }
}
