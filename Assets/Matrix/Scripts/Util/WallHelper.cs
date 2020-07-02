using System.Collections.Generic;
using UnityEngine;

public class WallHelper 
{
    public static Walls SegmentWalls(Walls walls, int startWall, int endWall)
    {
        Walls retVal = null;
        
        for(int i = startWall; i < endWall; i++)
        {
#if true
            for(int j = 0; j < walls.WallSegments[i].Length; j++)
            {
                WallSegment wallSegment1 = walls.WallSegments[i][j];
                WallSegment wallSegment2 = (j + 1) < walls.WallSegments[i].Length ? walls.WallSegments[i][j + 1] : walls.WallSegments[i][0];

                double angle = Mathf.Atan2((float)(wallSegment2.Y - wallSegment1.Y), (float)(wallSegment2.X - wallSegment2.X)) * 180 / Mathf.PI;

                if(angle != 0.0)
                {
                    Debug.Log($"Angle {angle}");

                    float mag = Vector2.Distance(new Vector2((float)wallSegment1.X, (float)wallSegment1.Y), new Vector2((float)wallSegment2.X, (float)wallSegment2.Y));

                    Debug.Log($"Magnitude {mag}");
                }         
            }
#else
            for (int j = 0; j < walls.WallSegments[i].Points.Count; j++)
            {
                Point wallSegment1 = walls.WallSegments[i].Points[j];
                Point wallSegment2 = (j + 1) < walls.WallSegments[i].Points.Count ? walls.WallSegments[i].Points[j + 1] : walls.WallSegments[i].Points[0];

                double angle = Mathf.Atan2((float)(wallSegment2.Y - wallSegment1.Y), (float)(wallSegment2.X - wallSegment2.X)) * 180 / Mathf.PI;

                if (angle != 0.0)
                {
                    Debug.Log($"Angle {angle}");

                    float mag = Vector2.Distance(new Vector2((float)wallSegment1.X, (float)wallSegment1.Y), new Vector2((float)wallSegment2.X, (float)wallSegment2.Y));

                    Debug.Log($"Magnitude {mag}");
                }
            }
#endif
        }


        return retVal;
    }

    public static List<WallSegment> RemoveArtifacts(WallSegment[] walls, int startVert, int endVert)
    {
        List<WallSegment> retVal = new List<WallSegment>();

        Debug.Log("********************");
        Debug.Log("**Remove Artifacts**");
        Debug.Log("********************");

#if false
        for (int i = startVert; i < endVert; i++)
        {
            WallSegment wallSegment1 = walls[i];
            WallSegment wallSegment2 = walls[i + 1];

            Debug.Log($"Segment [{i}] [{wallSegment1.X}],[{wallSegment1.Y}]");
            Debug.Log($"Segment [{i + 1}] [{wallSegment2.X}],[{wallSegment2.Y}]");

            double angle = Mathf.Atan2((float)(wallSegment2.Y - wallSegment1.Y), (float)(wallSegment2.X - wallSegment2.X)) * 180 / Mathf.PI;

            if (angle != 0.0)
            {
                Debug.Log($"Angle {angle}");

                float mag = Vector2.Distance(new Vector2((float)wallSegment1.X, (float)wallSegment1.Y), new Vector2((float)wallSegment2.X, (float)wallSegment2.Y));

                Debug.Log($"Magnitude {mag}");

                if(mag < 8.0f)
                {
                    Debug.Log("Possible Artifact");
                }
            }
        }
#endif

        return retVal;
    }

}
