using Doozy.Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDataManager : MonoBehaviour
{
    public static WallDataManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Transform WallDataParent;
    public GameObject WallInfoPrefab;

    Walls Walls;
    List<DrawWall> DrawWalls;

    public int CurrentVertex = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(Walls walls)
    {
        Walls = walls;
        DrawWalls = new List<DrawWall>();

        for (int i = 0; i < Walls.WallSegments.Length; i++)
        {
            DrawWall wall = new DrawWall
            {
                DrawMe = false,
                StartIndex = 0,
                EndIndex = Walls.WallSegments[i].Length
            };

            for (int j = 0; j < Walls.WallSegments[i].Length; j++)
            {
                wall.Points.Add(new Vector3((float)Walls.WallSegments[i][j].X, (float)Walls.WallSegments[i][j].Y, 0.0f));
            }

            DrawWalls.Add(wall);

            CreateWallInfo($"Wall {i}", wall);
        }
    }

    void CreateWallInfo(string name, DrawWall drawWall)
    {
        GameObject go = Instantiate(WallInfoPrefab, WallDataParent);
        WallInfo wallInfo = go.GetComponent<WallInfo>();
        wallInfo.Init(name, drawWall);
    }

    public void Redraw()
    {
        WallLoader.Instance.Redraw(DrawWalls);
    }

    public void RemoveArtifacts()
    {
        GameEventMessage.SendEvent(eMessages.PROGRESS_ARTIFACTS.ToString());
    }

    public void WallSegments()
    {
        GameEventMessage.SendEvent(eMessages.PROGRESS_WALLSEGMENT.ToString());
    }
}
