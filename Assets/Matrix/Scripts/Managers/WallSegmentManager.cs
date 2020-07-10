using Doozy.Engine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WallSegmentManager : BaseMono
{
    public static WallSegmentManager Instance;
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

    WallData Walls;    
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
        ConsoleLogging = ProgressManager.GetConsoleLogging(eStateDebuging.WALLSEGMENTS);

        if (ProgressManager != null)
        {
            DataPath = ProgressManager.DataPath;
            InFile = ProgressManager.Geometry_ArtifactsRemoved;
            OutFile = ProgressManager.Geometry_WallSegments;
            Headless = ProgressManager.WallSegment_Headless;
            FullAuto = ProgressManager.WallSegment_FullAuto;
            WallByWall = ProgressManager.WallSegment_WallByWall;
            PointByPoint = ProgressManager.WallSegment_PointByPoint;
        }

        if (FullAuto)
            ConsoleLogging = eConsoleLogging.NA;

        ProgressManager.Progress.SetState(eProgressState.WALLSEGMENTS.ToString(), ConsoleLogging);
        GameEventMessage.SendEvent(eMessages.PROGRESS_WALLSEGMENT_INFILE.ToString());
    }

    public void LoadInput()
    {
        string file = $"{DataPath}{InFile}";
        string json = FileIO.OpenFile(file);

        if (!string.IsNullOrEmpty(json))
        {
            Walls = JsonConvert.DeserializeObject<WallData>(json);

            if (Walls != null)
            {
                CurrentWall = 0;

                SetText(0, Walls.WallSegments.Count.ToString());
                SetText(2, Walls.WallSegments[CurrentWall].Points.Count.ToString());

                SetCurrentSegmentText(CurrentWall);

                if (FullAuto)
                {
                    GameEventMessage.SendEvent(eMessages.PROGRESS_WALLSEGMENT_PROCESS.ToString());
                }
            }
            else
                ProgressManager.Progress.Log($"LoadInput - Deserialize FAIL - [{file}]", eConsoleLogging.ERROR);
        }
        else
            ProgressManager.Progress.Log($"LoadInput - FileOpen FAIL - [{file}]", eConsoleLogging.ERROR);
    }

    public void SubtractSegment()
    {
        CurrentWall = CurrentWall == 0 ? CurrentWall = Walls.WallSegments.Count - 1 : CurrentWall - 1;
        SetCurrentSegmentText(CurrentWall);

        int points = Walls.WallSegments[CurrentWall].Points.Count;
        SetText(2, points.ToString());
    }

    public void AddSegment()
    {
        CurrentWall = CurrentWall == Walls.WallSegments.Count - 1 ? 0 : CurrentWall + 1;
        SetCurrentSegmentText(CurrentWall);

        int points = Walls.WallSegments[CurrentWall].Points.Count;
        SetText(2, points.ToString());

        if (FullAuto)
        {
            if (CurrentWall != 0)
                GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_PROCESS.ToString());
            else
                GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_WRITE_WALLS.ToString());
        }
    }

    public void CreateButtons(int segmentIndex, eArtifactExceptions exception)
    {
        Point segment = Walls.WallSegments[CurrentWall].Points[segmentIndex];
        GameObject go = Instantiate(ButtonPointPrefab, ButtonParent);

        PointButton pointButton = go.GetComponent<PointButton>();
        Color color = exception == eArtifactExceptions.NA ? Color.white : Color.red;
        pointButton.Init(segmentIndex, $"P{segmentIndex + 1} ({segment.X},{segment.Y}) [{exception.ToString()}]", color);
    }

    public void Process()
    {
        Log("ProcessPoints");

        bool exception = false;
        DestroyPoints();
        DestroyButtons();

        if (Points == null)
            Points = new List<WallSeg>();
        else
            Points.Clear();

        float xOffset = Walls.WallSegments[CurrentWall].Points[0].X;
        float yOffset = Walls.WallSegments[CurrentWall].Points[0].Y;

        WallSeg w = new WallSeg()
        {
            Points = new List<Point>()
        };

        Log("***************Progress Points Start***********************");
        for (int i = 0; i < Walls.WallSegments[CurrentWall].Points.Count; i++)
        {
            Log($"Current Wall Segment: [{CurrentWall + 1}] P1: [{i + 1}] P2: [{i + 2}]");
            CreateButtons(i, Walls.WallSegments[CurrentWall].Points[i].AE);

            WallSeg s = Walls.WallSegments[CurrentWall];
            Point sP = Walls.WallSegments[CurrentWall].Points[i];
            Point p = new Point()
            {
                AE = Walls.WallSegments[CurrentWall].Points[i].AE,
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
                Walls.WallSegments[CurrentWall].PointCount = Walls.WallSegments[CurrentWall].Points.Count;
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
        switch (ArtifactExceptions.GetDirection(angle))
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
        Process();
    }

    public void ZoomMinus()
    {
        xMod = xMod > 1.0f ? xMod - 1.0f : 1.0f;
        yMod = yMod > 1.0f ? yMod - 1.0f : 1.0f;

        DestroyPoints();
        Process();
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

    public void SegmentWall()
    {
        Log("************Fix Start******************");
        Log($"Fix CurrentWall: {CurrentWall}");

        int currentPoint = 0;
        bool x = false;
        Point point;
        int segmentPointCount = 3;
        int minPoints = 5;
        bool done = false;
        int walls = Walls.WallSegments.Count;
        if (Walls.WallSegments[CurrentWall].Points.Count > 5)
        {
            WallSeg copySegment = new WallSeg()
            {
                Points = new List<Point>()
            };

            foreach (Point p in Walls.WallSegments[CurrentWall].Points)
            {
                if (p.PointId >= 0)
                    copySegment.Points.Add(p);
            }

            int count = 0;
            do
            {
                WallSeg segment = new WallSeg()
                {
                    WallId = Walls.WallSegments.Count,
                    Points = new List<Point>()
                };

                Point p1 = copySegment.Points[currentPoint];
                p1.RemoveMe = true;

                point = new Point()
                {
                    X = p1.X,
                    Y = p1.Y,
                };

                segment.Points.Add(point);

                do
                {
                    if(segment.Points.Count == 1)
                        currentPoint = GetNextPointMax(x, segment.Points[0], currentPoint, copySegment.Points);
                    else
                        currentPoint = GetNextPointMin(x, segment.Points[1], currentPoint, copySegment.Points);

                    if (currentPoint != -1)
                    {
                        point = new Point()
                        {
                            X = copySegment.Points[currentPoint].X,
                            Y = copySegment.Points[currentPoint].Y,
                            DirectionFromPrevPoint = copySegment.Points[currentPoint].DirectionFromPrevPoint,
                            DistanceToPrevPoint = copySegment.Points[currentPoint].DistanceToPrevPoint,
                        };

#if false
                        if (segment.Points.Count == 2)
                        {
                            if (point.DistanceToPrevPoint > 9.0f)
                            {
                                switch (point.DirectionFromPrevPoint)
                                {
                                    case eDirections.NORTH:
                                        point.Y = segment.Points[1].Y + 9.0f;
                                        break;
                                    case eDirections.EAST:
                                        point.X = segment.Points[1].X + 9.0f;
                                        break;
                                    case eDirections.SOUTH:
                                        point.Y = segment.Points[1].Y - 9.0f;
                                        break;
                                    case eDirections.WEST:
                                        point.X = segment.Points[1].X - 9.0f;
                                        break;
                                }
                            }

                        }
#endif

                        segment.Points.Add(point);
                    }
                    else
                        currentPoint = 0;

                    x = !x;
                }
                while (segment.Points.Count < segmentPointCount);

                int index1 = x ? 2 : 0;
                int index2 = x ? 0 : 2;

                point = new Point()
                {
                    X = segment.Points[index1].X,
                    Y = segment.Points[index2].Y
                };
                segment.Points.Add(point);

                point = new Point()
                {
                    X = segment.Points[0].X,
                    Y = segment.Points[0].Y
                };
                segment.Points.Add(point);

                Walls.WallSegments.Add(segment);
                count++;
                currentPoint = 0;

                copySegment.Points.Clear();

                foreach (Point p in Walls.WallSegments[CurrentWall].Points)
                {
                    if (!p.RemoveMe)
                        copySegment.Points.Add(p);
                }

                float angle = ArtifactExceptions.GetAngle(new Vector2(copySegment.Points[0].X, copySegment.Points[0].Y), new Vector2(copySegment.Points[1].X, copySegment.Points[1].Y));
                x = angle == 90.0f;
#if false
            }

            while(copySegment.Points.Count >= minPoints);
#else
                done = Walls.WallSegments.Count == walls + 2;
            }
            while (!done);
#endif

        }
        
        Log("************Fix End******************");
        //GameEventMessage.SendEvent(eMessages.PROGRESS_WALLSEGMENT_PROCESS.ToString());
    }

    int GetNextPointMax(bool x, Point p1, int currentPoint, List<Point> points)
    {
        int retVal = -1;
        //Point p1 = points[currentPoint];
        float farthest = 0.0f;
        int prevIndex = -1;
        for(int i = 1; i < points.Count; i++)
        {
            if (i > currentPoint)            
            {   
                float distance = ArtifactExceptions.GetDistance(new Vector2(p1.X, p1.Y), new Vector2(points[i].X, points[i].Y));
                float angle = ArtifactExceptions.GetAngle(new Vector2(p1.X, p1.Y), new Vector2(points[i].X, points[i].Y));
                eDirections direction = ArtifactExceptions.GetDirection(angle);

                if (x)
                {
                    if (points[i].X == p1.X)
                    {
                        if (distance > farthest)
                        {
                            farthest = distance;
                            points[i].DistanceToPrevPoint = distance;
                            points[i].DirectionFromPrevPoint = direction;
                            points[i].RemoveMe = true;

                            if (prevIndex > 0)
                            {
                                points[prevIndex].DistanceToPrevPoint = 0.0f;
                                points[prevIndex].RemoveMe = false;
                            }
                            
                            prevIndex = i;

                            retVal = i;
                        }
                    }
                }
                else
                {
                    if (points[i].Y == p1.Y)
                    {
                        if (distance > farthest)
                        {
                            farthest = distance;
                            points[i].DistanceToPrevPoint = distance;
                            points[i].DirectionFromPrevPoint = direction;
                            points[i].RemoveMe = true;

                            if (prevIndex > 0)
                                points[prevIndex].RemoveMe = false;

                            prevIndex = i;

                            retVal = i;
                        }
                    }
                }
            }
        }

        return retVal;
    }

    int GetNextPointMin(bool x, Point p1, int currentPoint, List<Point> points)
    {
        int retVal = -1;
        //Point p1 = points[currentPoint];
        float min = 10.0f;
        int prevIndex = -1;
        for (int i = 1; i < points.Count; i++)
        {
            if (i > currentPoint)            
            {
                float distance = ArtifactExceptions.GetDistance(new Vector2(p1.X, p1.Y), new Vector2(points[i].X, points[i].Y));
                float angle = ArtifactExceptions.GetAngle(new Vector2(p1.X, p1.Y), new Vector2(points[i].X, points[i].Y));
                eDirections direction = ArtifactExceptions.GetDirection(angle);

                if (x)
                {
                    if (points[i].X == p1.X)
                    {
                        if (distance < min)
                        {
                            min = distance;
                            points[i].DistanceToPrevPoint = distance;
                            points[i].DirectionFromPrevPoint = direction;
                            points[i].RemoveMe = true;

                            if (prevIndex > 0)
                            {
                                points[prevIndex].DistanceToPrevPoint = 0.0f;
                                points[prevIndex].RemoveMe = false;
                            }

                            prevIndex = i;

                            retVal = i;
                        }
                    }
                }
                else
                {
                    if (points[i].Y == p1.Y)
                    {
                        if (distance < min)
                        {
                            min = distance;
                            points[i].DistanceToPrevPoint = distance;
                            points[i].DirectionFromPrevPoint = direction;
                            points[i].RemoveMe = true;

                            if (prevIndex > 0)
                                points[prevIndex].RemoveMe = false;

                            prevIndex = i;

                            retVal = i;
                        }
                    }
                }
            }
        }

        return retVal;
    }

    public void WriteWalls()
    {
        Log("================================");
        Log($"Write Walls - To File: [{OutFile}]");

#if FALSE
        string json = JsonConvert.SerializeObject(WallData);
        byte[] bytes = Encoding.ASCII.GetBytes(json);
        string dataFile = $"{DataPath}{OutFile}";
        FileIO.WriteFile(dataFile, bytes);
#endif

        Log("==================================");

        GameEventMessage.SendEvent(eMessages.PROGRESS_WALLSEGMENT_CLOSE.ToString());
        GameEventMessage.SendEvent(eMessages.PROGRESS_WRITE.ToString());
        GameEventMessage.SendEvent(eMessages.PROGRESS_WALLSEGMENT_COMPLETE.ToString());
    }
}
