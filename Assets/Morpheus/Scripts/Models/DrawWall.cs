using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWall
{
	public bool DrawMe;
	public int StartIndex;
	public int EndIndex;
	public Color Color;

	public List<Vector3> Points;

	public DrawWall()
	{
		Points = new List<Vector3>();
		Color = Color.white;
	}
}

