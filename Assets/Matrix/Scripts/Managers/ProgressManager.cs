using Doozy.Engine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ProgressManager : BaseMono
{
    public static ProgressManager Instance;
    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public string ProgressDataFile;
    public string Geometry_Copy;
    public string Geometry_ArtifactsRemoved;
    public string Geometry_WallSegments;
    public eProgressState State;
    
    [SerializeField]
    public List<ProgressStateToCallback> ProgressStateToCallback;

    public Progress Progress;

    public Transform ProgressParent;
    public List<StatePrefab> ProgressManagerStatePrefabs;

    public bool DeleteProgressOnLoad;
    public string DataPath;
    string DataFile;

    public float MinPointDistance;

    public bool Artifact_Headless;
    public bool Artifacts_FullAuto;
    public bool Artfifact_WallByWall;
    public bool Artifacts_PointByPoint;

    public bool WallSegment_Headless;
    public bool WallSegment_FullAuto;
    public bool WallSegment_WallByWall;
    public bool WallSegment_PointByPoint;

    [SerializeField]
    public List<ConsoleLogging> ConsoleLoggingList;
        
    // Start is called before the first frame update
    void Start()
    {
        State = eProgressState.IDLE;

        ConsoleLogging = GetConsoleLogging(eStateDebuging.PROGRESS);

        if (ArtifactManager.Instance != null)
            ArtifactManager.Instance.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
           
    }

    void SetState(string state)
    {
        SetState((eProgressState)Enum.Parse(typeof(eProgressState), state));
    }

    void SetState(eProgressState state)
    {
        State = state;

        if (Progress != null)
            Progress.SetState(State.ToString(), ConsoleLogging);
    }

    public void SetPath()
    {
        DataPath  = ((WallLoader)CallbackObject).DataPath;
        DataFile = $"{DataPath}{ProgressDataFile}";
        Debug.Log($"ProgressManager - SetPath DataPath: {DataPath} ");
        GameEventMessage.SendEvent(eMessages.PROGRESS_LOAD.ToString());        
    }

    public void Load()
    {   
        Debug.Log($"ProgressManager - Load path: {DataFile}");

        if(DeleteProgressOnLoad)
            File.Delete(DataFile);

        string json = FileIO.OpenFile(DataFile);
        if(!string.IsNullOrEmpty(json))
        { 
            Progress = JsonConvert.DeserializeObject<Progress>(json);
            SetState(Progress.CurrentState);

            GameEventMessage.SendEvent(eMessages.PROGRESS_CONTROLLER.ToString());
        }
        else
        {
            GameEventMessage.SendEvent(eMessages.PROGRESS_CREATE.ToString());
        }
    }

    public void Create()
    {
        string json = FileIO.OpenFile(DataFile);

        if(string.IsNullOrEmpty(json))
        {
            State = eProgressState.CREATE;
            Progress = new Progress(State.ToString(), MinPointDistance);

            Progress.Log($"ProgressManager - Create path: {DataFile}", ConsoleLogging);

            WriteProgress();

            GameEventMessage.SendEvent(eMessages.PROGRESS_CONTROLLER.ToString());
        }
        else
        {
            GameEventMessage.SendEvent(eMessages.PROGRESS_LOAD.ToString());
        }
    }

    public void Controller()
    {
        Progress.Log($"ProgressManager - Controller", ConsoleLogging);
        eProgressState state = (eProgressState)Enum.Parse(typeof(eProgressState), Progress.CurrentState);

        foreach (ProgressStateToCallback stateToCallback in ProgressStateToCallback)
        {
            if(state == stateToCallback.State && !stateToCallback.Deferred)
                GameEventMessage.SendEvent(stateToCallback.Callback.ToString());
        }
    }

    public void Log()
    {
        BaseMono b = (BaseMono)CallbackObject;
        Progress.Log(b.LogMessage, b.ConsoleLogging);
    }

    public void WriteProgress()
    {
        string progressJson = JsonConvert.SerializeObject(Progress);
        byte[] bytes = Encoding.ASCII.GetBytes(progressJson);

        FileIO.WriteFile(DataFile, bytes);
    }
    
    GameObject GetStatePrefab(eProgressState state)
    {
        GameObject retVal = null;
        foreach(StatePrefab prefab in ProgressManagerStatePrefabs)
        {
            if(prefab.State == state)
            {
                retVal = prefab.Prefab;
                break;
            }
        }

        return retVal;
    }

    public void CopyGeometry()
    {
        string geoFile = $"{DataPath}geometry.json";
        string copyFile = $"{DataPath}{Geometry_Copy}";

        string json = FileIO.OpenFile(copyFile);

        if(string.IsNullOrEmpty(json))
        {
            string geoFileJson = FileIO.OpenFile(geoFile);

            if(!string.IsNullOrEmpty(geoFileJson))
            {                
                byte[] bytes = Encoding.ASCII.GetBytes(geoFileJson);
                FileIO.WriteFile(copyFile, bytes);
            }
            else
            {
                Progress.Log("*** Missing Goemetry.json ***", eConsoleLogging.ERROR);
            }
        }
        else
        {

        }    
    }

    public eConsoleLogging GetConsoleLogging(eStateDebuging stateDebuging)
    {
        eConsoleLogging retVal = eConsoleLogging.NA;
        if (ConsoleLoggingList != null)
        {
            foreach (ConsoleLogging logging in ConsoleLoggingList)
            {
                if (logging.StateDebuging == stateDebuging)
                {
                    retVal = logging.LoggingType;
                    break;
                }
            }
        }

        return retVal;
    }

    public void Artifacts()
    {
        Progress.Log($"ProgressManager - Loading Artifacts", ConsoleLogging);

        GameObject prefab = GetStatePrefab(eProgressState.ARTIFACTS);
        GameObject go = Instantiate(prefab, ProgressParent);

        GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS.ToString(), this);

        WriteProgress();
    }

    public void ArtifactComplete()
    {
        Progress.Log("ProgressManager - Artifact Complete");
        SetState(eProgressState.WALLSEGMENTS);
        WriteProgress();
    } 
    
    public void WallSegments()
    {
        Progress.Log($"ProgressManager - Loading WallSegments", ConsoleLogging);
        GameObject prefab = GetStatePrefab(eProgressState.WALLSEGMENTS);
        GameObject go = Instantiate(prefab, ProgressParent);

        GameEventMessage.SendEvent(eMessages.PROGRESS_WALLSEGMENT_INIT.ToString(), this);

        WriteProgress();
    }
}
