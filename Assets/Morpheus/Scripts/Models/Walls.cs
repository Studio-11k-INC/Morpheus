using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WallSegment
{
	public float X { get; set; }
	public float Y { get; set; }	
	public eArtifactExceptions AE { get; set; }
}

public partial class Walls
{	
	public long Width { get; set; }
	public long Height { get; set; }	
	public WallSegment[][] WallSegments { get; set; }
}

public class Point
{
	public float X { get; set; }
	public float Y { get; set; }
	public eArtifactExceptions AE { get; set; }
}

public class WallSeg
{
	public List<Point> Points { get; set; }	
}

public class WallData
{
	public float Width { get; set; }
	public float Height { get; set; }
	public List<WallSeg> WallSegments { get; set; }
}


