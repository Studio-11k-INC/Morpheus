using Doozy.Engine;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class WallLoader : BaseMono
{
	public static WallLoader Instance;

	public override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	Walls Walls;	
	public GameObject Sphere;
	public List<LineRenderer> LineRenderers;
	public GameObject LineRendererPrefab;
	public Transform LineRendererParent;
	public string DataPath;
	// Start is called before the first frame update
	void Start()
	{
		DataPath = $"{Application.dataPath}/../Data/Test/";

		Walls = LoadWalls($"{DataPath}geometry.json");		
		WallDataManager.Instance.Init(Walls);

		GameEventMessage.SendEvent(eMessages.PROGRESS_SETPATH.ToString(), Instance);
	}

	// Update is called once per frame
	void Update()
	{

	}

	public Walls LoadWalls(string path)
	{
		Walls retVal = null;
		StreamReader reader;

		try
		{
			if (File.Exists(path))
			{
				FileStream stream = File.Open(path, FileMode.Open);
				reader = new StreamReader(stream);
				string json = reader.ReadToEnd();
				reader.Close();
				stream.Close();

				retVal = JsonConvert.DeserializeObject<Walls>(json);
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
		}

		return retVal;
	}

	public void SaveWalls(Walls walls)
    {

    }

	public void CreateWalls(Walls walls)
	{
		List<Vector3> vectors = new List<Vector3>();

#if true
		for(int i = 0; i < walls.WallSegments.Length; i++)
		{
			for (int j = 0; j < walls.WallSegments[i].Length; j++)
			{
				WallSegment segment = walls.WallSegments[i][j];
				float x = (float)(segment.X * 0.1f);
				float y = (float)(segment.Y * 0.1f);
#if false
				GameObject sphere = Instantiate(Sphere);				
				sphere.transform.position = new Vector3(x, y);
				sphere.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
#else
				Vector3 vector = new Vector3(x, y);
				vectors.Add(vector);

				//LineRenderer.SetPosition(j, vector);
#endif
			}			
		}
#else
		for (int i = 0; i < walls.WallSegments.Count; i++)
		{
			for (int j = 0; j < walls.WallSegments[i].Points.Count; j++)
			{
				Point segment = walls.WallSegments[i].Points[j];
				float x = (float)(segment.X * 0.1f);
				float y = (float)(segment.Y * 0.1f);
#if false
				GameObject sphere = Instantiate(Sphere);				
				sphere.transform.position = new Vector3(x, y);
				sphere.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
#else
				Vector3 vector = new Vector3(x, y);
				vectors.Add(vector);

				//LineRenderer.SetPosition(j, vector);
#endif
			}
		}
#endif


		//LineRenderer.positionCount = vectors.Count;
		//LineRenderer.SetPositions(vectors.ToArray());

	}

	public void Redraw(List<DrawWall> drawWalls)
	{
		DrawWalls(drawWalls, 0.0f);		
	}

	public void DrawWalls(List<DrawWall> drawWalls, float z)
	{ 
		List<Vector3> draw = new List<Vector3>();

		for (int i = LineRendererParent.childCount - 1; i >= 0; i--)
			Destroy(LineRendererParent.GetChild(i).gameObject);

		if (LineRenderers == null)
			LineRenderers = new List<LineRenderer>();
		else
			LineRenderers.Clear();

		foreach (DrawWall wall in drawWalls)
		{
			if (wall.DrawMe)
			{
				GameObject go = Instantiate(LineRendererPrefab, LineRendererParent);
				LineRenderer lineRenderer = go.GetComponent<LineRenderer>();
				LineRenderers.Add(lineRenderer);				

				for (int i = wall.StartIndex; i < wall.EndIndex; i++)
				{
					Vector3 v = new Vector3(wall.Points[i].x * 0.1f, wall.Points[i].y * 0.1f, z);
					draw.Add(v);
					
				}
				lineRenderer.positionCount = draw.Count;
				lineRenderer.SetPositions(draw.ToArray());

				draw.Clear();
			}
		}

		
	}
}
