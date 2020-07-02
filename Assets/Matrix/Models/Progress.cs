using System;
using System.Collections.Generic;

public class ProgressLog
{
    public ProgressLog(string log)
    {
        DateStamp = DateTime.Now.ToString();
        Log = log;
    }

    public string DateStamp { get; set; }
    public string Log { get; set; }
}

public class Progress
{
    public Progress(string state)
    {
        CreateDate = DateTime.Now.ToString();
        LastModified = CreateDate;
        Logs = new List<ProgressLog>();

        SetState(state);
    }

    public string CreateDate { get; set; }
    public string LastModified { get; set; }
    public string CurrentState { get; set; }
    public List<ProgressLog> Logs { get; set; }

    public void SetState(string state)
    {
        SetLog($"SetState - From: {CurrentState} To: {state}");
        CurrentState = state;
    }
    public void SetLog(string log)
    {
        ProgressLog entity = new ProgressLog(log);
        Logs.Add(entity);
    }
}
