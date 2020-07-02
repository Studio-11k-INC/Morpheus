using Doozy.Engine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public eProgressState State;
    
    [SerializeField]
    public List<ProgressStateToCallback> ProgressStateToCallback;

    public Progress Progress;

    public Transform ProgressParent;
    public List<StatePrefab> ProgressManagerStatePrefabs;

    public string DataPath;
    string DataFile;

    public bool Artifacts_PointByPoint;
    // Start is called before the first frame update
    void Start()
    {
        State = eProgressState.IDLE;

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
            Progress.SetState(State.ToString());
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
        Debug.Log($"ProgressManager - Create path: {DataFile}");

        string json = FileIO.OpenFile(DataFile);

        if(string.IsNullOrEmpty(json))
        {
            State = eProgressState.CREATE;
            Progress = new Progress(State.ToString());

            string progressJson = JsonConvert.SerializeObject(Progress);
            byte[] bytes = Encoding.ASCII.GetBytes(progressJson);

            FileIO.WriteFile(DataFile, bytes);
            
            GameEventMessage.SendEvent(eMessages.PROGRESS_CONTROLLER.ToString());
        }
        else
        {
            GameEventMessage.SendEvent(eMessages.PROGRESS_LOAD.ToString());
        }
    }

    public void Controller()
    {
        Debug.Log($"ProgressManager - Controller");
        eProgressState state = (eProgressState)Enum.Parse(typeof(eProgressState), Progress.CurrentState);

        foreach (ProgressStateToCallback stateToCallback in ProgressStateToCallback)
        {
            if(state == stateToCallback.State && !stateToCallback.Deferred)
                GameEventMessage.SendEvent(stateToCallback.Callback.ToString());
        }
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
                Debug.LogError("*** Missing Goemetry.json ***");
            }
        }
        else
        {

        }
    }

    public void Artifacts()
    {
        Debug.Log($"ProgressManager - Artifacts");

        State = eProgressState.ARTIFACTS;
        Progress.SetState(State.ToString());

        GameObject prefab = GetStatePrefab(State);
        GameObject go = Instantiate(prefab, ProgressParent);

        GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS.ToString(), this);
    }
}
