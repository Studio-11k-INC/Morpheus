using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class ArtifactExceptions
{
    public static float[] DirectionsAngles = { 90.0f, -90.0f, 0.0f, 180.0f };
    public static float GetDistance(Vector2 p1, Vector2 p2)
    {
        float retVal = float.NaN;
        retVal = Vector2.Distance(new Vector2(p1.x, p1.y), new Vector2(p2.x, p2.y));
        return retVal;
    }

    public static float GetAngle(Vector2 p1, Vector2 p2)
    {
        float retVal = float.NaN;
        retVal = Mathf.Atan2((p2.y - p1.y), (p2.x - p1.x)) * 180 / Mathf.PI;
        return retVal;
    }

    public static eDirections GetDirection(float angle)
    {
        eDirections retVal = eDirections.NA;
        for (eDirections i = eDirections.NORTH; i < eDirections.NA; i++)
        {
            if(angle == DirectionsAngles[(int)i])
            {
                retVal = i;
            }
        }

        return retVal;
    }

    public static string GetDirectionString(float angle)
    {
        string retVal = eDirections.NA.ToString();
        for (eDirections i = eDirections.NORTH; i < eDirections.NA; i++)
        {
            if (angle == DirectionsAngles[(int)i])
            {
                retVal = i.ToString();
            }
        }

        return retVal;
    }

    public static eArtifactExceptions GetArtifactException(Vector2 p1, Vector2 p2, float minDistance)
    {
        eArtifactExceptions retVal = eArtifactExceptions.NA;
       
        for (eArtifactExceptions i = eArtifactExceptions.AE1; i < eArtifactExceptions.END; i++)
        {
            if (VerifyExceptionByDirection((eDirections)(i - 1), p1, p2, minDistance))
            {
                retVal = i;
                break;
            }
        }
       
        return retVal;
    }

    public static bool VerifyExceptionByDirection(eDirections direction, Vector2 p1, Vector2 p2, float minDistance)
    {
        bool retVal = false;
        float angle = GetAngle(p1, p2);
        eDirections d = GetDirection(angle);
        if(d == direction)
        {
            float distance = GetDistance(p1, p2);
            
            if(distance < minDistance)
            {
                retVal = true;
            }
        }

        return retVal;
    }

    public static List<Point> FixArtifactExceptions(eArtifactExceptions exception, List<Point> Segments, int exceptionIndex)
    {
        List<Point> retVal = new List<Point>();

        switch(exception)
        {
            case eArtifactExceptions.AE1:
            {
                Segments.RemoveAt(exceptionIndex);
                Point p0 = exceptionIndex - 1 >= 0 ? Segments[exceptionIndex - 1] : Segments[Segments.Count - 1];
                p0.Y = Segments[exceptionIndex].Y;
                retVal = Segments;
            }
            break;
            case eArtifactExceptions.AE2:
            {
                Segments.RemoveAt(exceptionIndex);
                Point p0 = exceptionIndex - 1 >= 0 ? Segments[exceptionIndex - 1] : Segments[Segments.Count - 1];
                p0.Y = Segments[exceptionIndex].Y;                
                retVal = Segments;
            }
            break;
            case eArtifactExceptions.AE3:
                {
                    Segments.RemoveAt(exceptionIndex);
                    Point p0 = exceptionIndex - 1 >= 0 ? Segments[exceptionIndex - 1] : Segments[Segments.Count - 1];
                    p0.X = Segments[exceptionIndex].X;
                    retVal = Segments;
                }
                break;
            case eArtifactExceptions.AE4:
                {
                    Segments.RemoveAt(exceptionIndex);
                    Point p0 = exceptionIndex - 1 >= 0 ? Segments[exceptionIndex - 1] : Segments[Segments.Count - 1];
                    p0.X = Segments[exceptionIndex].X;
                    retVal = Segments;                    
                }
                break;

        }

        return retVal;
    }
}
