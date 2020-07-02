using System;

[Serializable]
public class ProgressStateToCallback
{
    public bool Deferred;
    public eProgressState State;
    public eMessages Callback;
}