using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ProgressLog
{
    public ProgressLog(int id, string log, eConsoleLogging consoleLog)
    {
        Id = id;
        DateStamp = DateTime.Now.ToString();
        Log = log;
        ConsoleLog = consoleLog;
    }

    public int Id { get; set; }
    public string DateStamp { get; set; }
    public string Log { get; set; }
    public eConsoleLogging ConsoleLog { get; set; }
}

public class Progress
{
    public static Progress Instance;
    public Progress() { }

    public Progress(string state, float minWallWidth)
    {
        Instance = this;
        CreateDate = DateTime.Now.ToString();
        LastModified = CreateDate;
        Logs = new List<ProgressLog>();

        MinWallWidth = minWallWidth;

        SetState(state);
    }

    public string CreateDate { get; set; }
    public string LastModified { get; set; }
    public string CurrentState { get; set; }
    public float MinWallWidth { get; set; }
    public List<ProgressLog> Logs { get; set; }    

    public void SetState(string state, eConsoleLogging consoleLog = eConsoleLogging.NA)
    {
        Log($"SetState - From: {CurrentState} To: {state}", consoleLog);
        CurrentState = state;
    }

    public void Log(string log, eConsoleLogging consoleLog = eConsoleLogging.NA)
    {
        ProgressLog entity = new ProgressLog(Logs.Count, log, consoleLog);
        Logs.Add(entity);

        switch (consoleLog)
        {
            case eConsoleLogging.DEBUG:
                Debug.Log(log);
                break;
            case eConsoleLogging.WARNING:
                Debug.LogWarning(log);
                break;
            case eConsoleLogging.ERROR:
                Debug.LogError(log);
                break;
        }
    }
}
