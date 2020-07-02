using Doozy.Engine;
using Newtonsoft.Json;
using System.Collections.Generic;
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

    bool PointByPoint;

    int CurrentSegment;
    int CurrentPoint;
    bool FixSegment;

    Walls Walls;
    WallData WallData;
    List<WallSeg> Points;

    public float xMod = 1.0f;
    public float yMod = 1.0f;

    float startX;
    float startY;

    public float swing;

    

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
        ProgressManager manager = (ProgressManager)CallbackObject;

        if(manager != null)
        {
            DataPath = manager.DataPath;
            InFile = manager.Geometry_Copy;
            OutFile = manager.Geometry_ArtifactsRemoved;
            PointByPoint = manager.Artifacts_PointByPoint;
        }

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
                CurrentSegment = 0;
                CurrentPoint = 0;               

                SetText(0, Walls.WallSegments.Length.ToString());
                SetText(2, Walls.WallSegments[CurrentSegment].Length.ToString());

                SetCurrentSegmentText(CurrentSegment);

                if(AutoRun)
                    GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_PROCESS.ToString());
            }
            else
                Debug.LogError($"LoadInput - Deserialize FAIL - [{file}]");
        }
        else
            Debug.LogError($"LoadInput - FileOpen FAIL - [{file}]");

        CreateWallData(Walls);
    }

    public void CreateWallData(Walls walls)
    {
        WallData = new WallData();
        WallData.WallSegments = new List<WallSeg>();

        for (int i = 0; i < Walls.WallSegments.Length; i++)
        {
            WallSeg wallSeg = new WallSeg()
            {                
                Points = new List<Point>()
            };

            for (int j = 0; j < Walls.WallSegments[i].Length; j++)
            {
                Point p = new Point()
                {
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
        CurrentSegment = CurrentSegment == 0 ? CurrentSegment = WallData.WallSegments.Count - 1 : CurrentSegment - 1;
        SetCurrentSegmentText(CurrentSegment);

        int points = WallData.WallSegments[CurrentSegment].Points.Count;
        SetText(2, points.ToString());
    }

    public void AddSegment()
    {
        CurrentSegment = CurrentSegment == WallData.WallSegments.Count - 1 ? 0 : CurrentSegment + 1;
        SetCurrentSegmentText(CurrentSegment);

        int points = WallData.WallSegments[CurrentSegment].Points.Count;
        SetText(2, points.ToString());
    }

    public void VerifyArtifactException(int segmentIndex)
    {
        WallSeg segment = WallData.WallSegments[CurrentSegment];
        Point point1 = WallData.WallSegments[CurrentSegment].Points[segmentIndex];
        Point point2 = segmentIndex < (WallData.WallSegments[CurrentSegment].Points.Count - 1) ? WallData.WallSegments[CurrentSegment].Points[segmentIndex + 1] : WallData.WallSegments[CurrentSegment].Points[0];

        point1.AE = ArtifactExceptions.GetArtifactException(new Vector2(point1.X, point1.Y), new Vector2(point2.X, point2.Y), 9.0f);
    }

    public void CreateButtons(int segmentIndex, eArtifactExceptions exception)
    {
        Point segment = WallData.WallSegments[CurrentSegment].Points[segmentIndex];
        GameObject go = Instantiate(ButtonPointPrefab, ButtonParent);

#if false
        TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();       

        go.name = $"P{segmentIndex + 1}";
        text.text = $"P{segmentIndex + 1} ({segment.X},{segment.Y}) [{exception.ToString()}]";

        text.color = exception == eArtifactExceptions.NA ? Color.white : Color.red;
#else

        PointButton pointButton = go.GetComponent<PointButton>();
        Color color = exception == eArtifactExceptions.NA ? Color.white : Color.red;
        pointButton.Init(segmentIndex, $"P{segmentIndex + 1} ({segment.X},{segment.Y}) [{exception.ToString()}]", color);
#endif
    }

    public void ProcessPoints()
    {
        Debug.Log("Process");

        DestroyPoints();
        DestroyButtons();

        if (Points == null)
            Points = new List<WallSeg>();
        else
            Points.Clear();

        float xOffset = WallData.WallSegments[CurrentSegment].Points[0].X;
        float yOffset = WallData.WallSegments[CurrentSegment].Points[0].Y;

        WallSeg w = new WallSeg()
        {
            Points = new List<Point>()
        };

        for (int i = 0; i < WallData.WallSegments[CurrentSegment].Points.Count; i++)
        {
            if(i < WallData.WallSegments[CurrentSegment].Points.Count - 1)
                VerifyArtifactException(i);

            CreateButtons(i, WallData.WallSegments[CurrentSegment].Points[i].AE);

            WallSeg s = WallData.WallSegments[CurrentSegment];
            Point sP = WallData.WallSegments[CurrentSegment].Points[i];
            Point p = new Point()
            {
                AE = WallData.WallSegments[CurrentSegment].Points[i].AE,
                X = (sP.X - xOffset) * xMod,
                Y = (sP.Y - yOffset) * yMod
            };

            w.Points.Add(p);
        }

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

        if (FixSegment && PointByPoint)
            GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_FIX.ToString());
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
        Debug.Log("Close");

        Destroy(gameObject);           
    }

    public void Fix()
    {        
        List<Point> FixedPoints;
        int i;
        for (i = 0; i < WallData.WallSegments[CurrentSegment].Points.Count; i++)
        {
            List<Point> Points = WallData.WallSegments[CurrentSegment].Points;
            WallSeg segment = WallData.WallSegments[CurrentSegment];
            if (segment.Points[i].AE != eArtifactExceptions.NA)
            {
                FixedPoints = ArtifactExceptions.FixArtifactExceptions(segment.Points[i].AE, Points, i);
                break;
            }
        }

        FixSegment = i != WallData.WallSegments[CurrentSegment].Points.Count;
        GameEventMessage.SendEvent(eMessages.PROGRESS_REMOVEARTIFACTS_PROCESS.ToString());        
    }
}
