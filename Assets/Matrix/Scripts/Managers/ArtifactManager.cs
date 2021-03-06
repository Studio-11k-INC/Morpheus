﻿using Doozy.Engine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactManager : BaseMono
{
    public static ArtifactManager Instance;
    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public bool AutoRun;
    public List<TextMeshProUGUI> TextGUI;
    public List<Scrollbar> Scrollbars;

    public Transform PointParent;
    public GameObject PointPrefab;

    public Transform ButtonParent;
    public GameObject ButtonPointPrefab;

    string DataPath;
    string InFile;
    string OutFile;

    bool Headless;
    bool FullAuto;
    bool WallByWall;
    bool PointByPoint;

    int CurrentWall;

    Walls Walls;
    WallData WallData;
    List<WallSeg> Points;

    public float xMod = 1.0f;
    public float yMod = 1.0f;

    float startX;
    float startY;

    public float swing;

    ProgressManager ProgressManager;    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetText(int index, string text)
    {
        TextGUI[index].text = text;
    }

    void SetCurrentSegmentText(int currentWall)
    {
        int Wall = currentWall + 1;
        string text = $"Wall {Wall}";
        SetText(1, text);
    }

    public void Init()
    {
        ProgressManager = (ProgressManager)CallbackObject;
        ConsoleLogging = ProgressManager.GetConsoleLogging(eStateDebuging.ARTIFACTS);

        if (ProgressManager != null)
        {            
            DataPath = ProgressManager.DataPath;
            InFile = ProgressManager.Geometry_Copy;
            OutFile = ProgressManager.Geometry_ArtifactsRemoved;
            Headless = ProgressManager.Artifact_Headless;
            FullAuto = ProgressManager.Artifacts_FullAuto;
            WallByWall = ProgressManager.Artfifact_WallByWall;            
            PointByPoint = ProgressManager.Artifacts_PointByPoint;
        }

        if (FullAuto)
            ConsoleLogging = eConsoleLogging.NA;

        ProgressManager.Progress.SetState(eProgressState.ARTIFACTS.ToString(), ConsoleLogging);
        GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_INFILE.ToString());        
    }

    public void LoadInput()
    {
        string file = $"{DataPath}{InFile}";
        string json = FileIO.OpenFile(file);

        if (!string.IsNullOrEmpty(json))
        {
            Walls = JsonConvert.DeserializeObject<Walls>(json);

            if (Walls != null)
            {
                CurrentWall = 0;

                SetText(0, Walls.WallSegments.Length.ToString());
                SetText(2, Walls.WallSegments[CurrentWall].Length.ToString());

                SetCurrentSegmentText(CurrentWall);

                CreateWallData(Walls);

                if (FullAuto)
                {                    
                    GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_PROCESS.ToString());
                }
            }
            else
                ProgressManager.Progress.Log($"LoadInput - Deserialize FAIL - [{file}]", eConsoleLogging.ERROR);
        }
        else
            ProgressManager.Progress.Log($"LoadInput - FileOpen FAIL - [{file}]", eConsoleLogging.ERROR);
    }

    public void CreateWallData(Walls walls)
    {
        WallData = new WallData()
        {
            Width = Walls.Width,
            Height = Walls.Height
        };

        WallData.WallSegments = new List<WallSeg>();

        for (int i = 0; i < Walls.WallSegments.Length; i++)
        {
            WallSeg wallSeg = new WallSeg()
            {
                WallId = i,
                Points = new List<Point>()
            };

            for (int j = 0; j < Walls.WallSegments[i].Length; j++)
            {
                Point p = new Point()
                {
                    PointId = j,
                    AE = eArtifactExceptions.NA,
                    X = Walls.WallSegments[i][j].X,
                    Y = Walls.WallSegments[i][j].Y
                };

                wallSeg.Points.Add(p);
            }

            WallData.WallSegments.Add(wallSeg);
        }
    }

    public void SubtractSegment()
    {
        CurrentWall = CurrentWall == 0 ? CurrentWall = WallData.WallSegments.Count - 1 : CurrentWall - 1;
        SetCurrentSegmentText(CurrentWall);

        int points = WallData.WallSegments[CurrentWall].Points.Count;
        SetText(2, points.ToString());
    }

    public void AddSegment()
    {
        CurrentWall = CurrentWall == WallData.WallSegments.Count - 1 ? 0 : CurrentWall + 1;
        SetCurrentSegmentText(CurrentWall);

        int points = WallData.WallSegments[CurrentWall].Points.Count;
        SetText(2, points.ToString());

        if (FullAuto)
        {
            if (CurrentWall != 0)
                GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_PROCESS.ToString());
            else
                GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_WRITE_WALLS.ToString());
        }
    }

    public bool VerifyArtifactException(int pointIndex)
    {
        bool retVal = false;
        Point point1 = WallData.WallSegments[CurrentWall].Points[pointIndex];
        Point point2 = pointIndex < (WallData.WallSegments[CurrentWall].Points.Count - 1) ? WallData.WallSegments[CurrentWall].Points[pointIndex + 1] : WallData.WallSegments[CurrentWall].Points[0];

        pointIndex += 1;
        point1.AE = ArtifactExceptions.GetArtifactException(new Vector2(point1.X, point1.Y), new Vector2(point2.X, point2.Y), ProgressManager.MinPointDistance, pointIndex, ConsoleLogging);

        ProgressManager.Instance.Progress.Log($"PointIndex: [{pointIndex}] Artifact Exception: [{point1.AE.ToString()}]", ConsoleLogging);

        retVal = point1.AE != eArtifactExceptions.NA;
        return retVal;
    }

    public void CreateButtons(int segmentIndex, eArtifactExceptions exception)
    {
        Point segment = WallData.WallSegments[CurrentWall].Points[segmentIndex];
        GameObject go = Instantiate(ButtonPointPrefab, ButtonParent);

        PointButton pointButton = go.GetComponent<PointButton>();
        Color color = exception == eArtifactExceptions.NA ? Color.white : Color.red;
        pointButton.Init(segmentIndex, $"P{segmentIndex + 1} ({segment.X},{segment.Y}) [{exception.ToString()}]", color);
    }

    public void ProcessPoints()
    {
        Log("ProcessPoints");

        bool exception = false;
        DestroyPoints();
        DestroyButtons();

        if (Points == null)
            Points = new List<WallSeg>();
        else
            Points.Clear();

        float xOffset = WallData.WallSegments[CurrentWall].Points[0].X;
        float yOffset = WallData.WallSegments[CurrentWall].Points[0].Y;

        WallSeg w = new WallSeg()
        {
            Points = new List<Point>()
        };

        Log("***************Progress Points Start***********************");
        for (int i = 0; i < WallData.WallSegments[CurrentWall].Points.Count; i++)
        {            
            Log($"Current Wall Segment: [{CurrentWall + 1}] P1: [{i + 1}] P2: [{i + 2}]");
            if(i < WallData.WallSegments[CurrentWall].Points.Count - 1)
                exception = exception || VerifyArtifactException(i);

            CreateButtons(i, WallData.WallSegments[CurrentWall].Points[i].AE);

            WallSeg s = WallData.WallSegments[CurrentWall];
            Point sP = WallData.WallSegments[CurrentWall].Points[i];
            Point p = new Point()
            {
                AE = WallData.WallSegments[CurrentWall].Points[i].AE,
                X = (sP.X - xOffset) * xMod,
                Y = (sP.Y - yOffset) * yMod
            };

            w.Points.Add(p);
        }
        Log("***************Progress Points End***********************");

        GameEventMessage.SendEvent(eMessages.PROGRESS_WRITE.ToString());        

        Points.Add(w);

        xOffset = 0.0f;
        yOffset = 0.0f;

        for (int i = 0; i < Points.Count; i++)
        {
            WallSeg s = Points[i];

            for (int j = 0; j < s.Points.Count; j++)
            {
                GameObject go = Instantiate(PointPrefab, PointParent);
                go.name = $"Point - {j + 1}";
                go.transform.localPosition = new Vector3(s.Points[j].X, s.Points[j].Y, 0.0f);

                PointLine pointLine = go.GetComponent<PointLine>();

                TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = $"{i}";

                if (s.Points[j].X > xOffset)
                    xOffset = (float)(s.Points[j].X / 2);

                if (s.Points[j].Y > yOffset)
                    yOffset = (float)(s.Points[j].Y / 2);

                if (s.Points[j].AE != eArtifactExceptions.NA)                
                    pointLine.Init(j, Color.red);                
                else
                    pointLine.Init(j, Color.green);

                Point p1 = s.Points[j];
                Point p2 = j < s.Points.Count - 1 ? s.Points[j + 1] : s.Points[0];
                ScalePoint(p1, p2, go.transform);
            }        
        }

        PointParent.localPosition = new Vector3(-xOffset, -yOffset, 0.0f);
        startX = PointParent.localPosition.x;
        startY = PointParent.localPosition.y;

        Scrollbars[0].numberOfSteps = (int)xOffset * 2;
        Scrollbars[0].value = 0.5f;

        Scrollbars[1].numberOfSteps = (int)yOffset * 2;
        Scrollbars[1].value = 0.5f;

        ScrollBarHorizontalChange();
        ScrollBarVerticalChange();

        if (FullAuto)
        {
            if (exception)
                GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_FIX.ToString());
            else
            {
                WallData.WallSegments[CurrentWall].PointCount = WallData.WallSegments[CurrentWall].Points.Count;
                GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_ADD_SEGMENT.ToString());
                GameEventMessage.SendEvent(eMessages.PROGRESS_WRITE.ToString());
            }
        }
    }

    void ScalePoint(Point p1, Point p2, Transform transform)
    {
        Vector2 v1 = new Vector2(p1.X, p1.Y);
        Vector2 v2 = new Vector2(p2.X, p2.Y);
        float angle = ArtifactExceptions.GetAngle(v1, v2);
        float distance = ArtifactExceptions.GetDistance(v1, v2);
        switch(ArtifactExceptions.GetDirection(angle))
        {
            case eDirections.NORTH:
                transform.localScale = new Vector3(1.0f, distance, 1.0f);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + (distance / 2), 0.0f);
                break;
            case eDirections.SOUTH:
                transform.localScale = new Vector3(1.0f, distance, 1.0f);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (distance / 2), 0.0f);
            break;
            case eDirections.EAST:
                transform.localScale = new Vector3(distance, 1.0f, 1.0f);
                transform.localPosition = new Vector3(transform.localPosition.x + (distance / 2), transform.localPosition.y, 0.0f);
            break;
            case eDirections.WEST:
                transform.localScale = new Vector3(distance, 1.0f, 1.0f);
                transform.localPosition = new Vector3(transform.localPosition.x - (distance / 2), transform.localPosition.y, 0.0f);
            break;
        }
    }

    public void ScrollBarHorizontalChange()
    {
        float x = 0.0f;

        x = startX + Scrollbars[0].value * Scrollbars[0].numberOfSteps;
        PointParent.localPosition = new Vector3(x, PointParent.localPosition.y, 0.0f);
    }

    public void ScrollBarVerticalChange()
    {
        float y = 0.0f;

        y = startY + Scrollbars[1].value * Scrollbars[1].numberOfSteps;
        PointParent.localPosition = new Vector3(PointParent.localPosition.x, y, 0.0f);
    }

    public void ZoomPlus()
    {
        xMod = xMod < 10.0f ? xMod + 1.0f : 10.0f;
        yMod = yMod < 10.0f ? yMod + 1.0f : 10.0f;

        DestroyPoints();
        ProcessPoints();
    }

    public void ZoomMinus()
    {
        xMod = xMod > 1.0f ? xMod - 1.0f : 1.0f;
        yMod = yMod > 1.0f ? yMod - 1.0f : 1.0f;

        DestroyPoints();
        ProcessPoints();
    }

    public void DestroyPoints()    
    {
        for (int i = PointParent.childCount - 1; i >= 0; i--)
            Destroy(PointParent.GetChild(i).gameObject);
    }

    public void DestroyButtons()
    {
        for (int i = ButtonParent.childCount - 1; i >= 0; i--)
            Destroy(ButtonParent.GetChild(i).gameObject);
    }

    public void Close()
    {
        Log("ArtifactManager Close");

        Destroy(gameObject);           
    }

    public void Fix()
    {
        Log("************Fix Start******************");
        Log($"Fix CurrentWall: {CurrentWall}");
        List<Point> FixedPoints;
        int i;

        for (i = WallData.WallSegments[CurrentWall].Points.Count - 1; i >= 0; i--)
        {
            List<Point> Points = WallData.WallSegments[CurrentWall].Points;
            WallSeg segment = WallData.WallSegments[CurrentWall];
            if (segment.Points[i].AE != eArtifactExceptions.NA)
            {
                Log($"Fixing Point: [{i + 1}], Artifact Exception: [{segment.Points[i].AE}], Point: [{segment.Points[i].X}, {segment.Points[i].Y}]");
                FixedPoints = ArtifactExceptions.FixArtifactExceptions(segment.Points[i].AE, Points, i, ConsoleLogging);
                break;
            }
        }
        Log("************Fix End******************");

        GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_PROCESS.ToString());
    }  
    
    public void WriteWalls()
    {
        Log("================================");
        Log($"Write Walls - To File: [{OutFile}]");

        string json = JsonConvert.SerializeObject(WallData);
        byte[] bytes = Encoding.ASCII.GetBytes(json);
        string dataFile = $"{DataPath}{OutFile}";
        FileIO.WriteFile(dataFile, bytes);

        Log("==================================");

        GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_CLOSE.ToString());
        GameEventMessage.SendEvent(eMessages.PROGRESS_WRITE.ToString());
        GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_COMPLETE.ToString());
    }
}
