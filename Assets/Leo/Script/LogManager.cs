using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public Text LogTextSpace;
    public int queueSize = 15;
    string myLog;
    Queue myLogQueue = new Queue();

    void Start()
    {
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type != LogType.Error)
            return;
        myLog = logString;
        string newString = "\n [" + type + "] : " + myLog;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
        myLog = string.Empty;
        if (myLogQueue.Count > queueSize)
        {
            myLogQueue.Dequeue();
        }
        foreach (string mylog in myLogQueue)
        {
            myLog += mylog;
        }
        LogTextSpace.text = myLog;
    }
}
