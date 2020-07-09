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

    public static eArtifactExceptions GetArtifactException(Vector2 p1, Vector2 p2, float minDistance, int pointIndex, eConsoleLogging consoleLogging)
    {
        eArtifactExceptions retVal = eArtifactExceptions.NA;
        ProgressManager.Instance.Progress.Log($"PointIndex: [{pointIndex}], P1: [{p1}], P2: [{p2}], Min Distance: [{minDistance}]", consoleLogging);

        for (eArtifactExceptions i = eArtifactExceptions.AE1; i < eArtifactExceptions.END; i++)
        {
            if (VerifyExceptionByDirection((eDirections)(i - 1), p1, p2, minDistance, pointIndex, consoleLogging))
            {                
                retVal = i;
                break;
            }
        }
       
        return retVal;
    }

    public static bool VerifyExceptionByDirection(eDirections direction, Vector2 p1, Vector2 p2, float minDistance, int pointIndex, eConsoleLogging consoleLogging)
    {
        bool retVal = false;
        float angle = GetAngle(p1, p2);
        eDirections d = GetDirection(angle);
        float distance = 0.0f;
        if (d == direction)
        {
            distance = GetDistance(p1, p2);
            
            if(distance < minDistance)
            {
                ProgressManager.Instance.Progress.Log($"PointIndex: [{pointIndex}] P1: [{p1}], Direction: [{d}], Distance: [{distance} < {minDistance}]", consoleLogging);
                retVal = true;
            }
        }

        return retVal;
    }

    public static List<Point> FixArtifactExceptions(eArtifactExceptions exception, List<Point> segments, int exceptionIndex, eConsoleLogging consoleLogging)
    {
        List<Point> retVal = new List<Point>();

        ProgressManager.Instance.Progress.Log($"^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", consoleLogging);
        ProgressManager.Instance.Progress.Log($"Exception: [{exception}]", consoleLogging);

        switch (exception)
        {
            case eArtifactExceptions.AE1:
            {
                ProgressManager.Instance.Progress.Log($"Remove Point At: [{exceptionIndex + 1}]", consoleLogging);
                segments.RemoveAt(exceptionIndex);
#if false
                    Point p0 = exceptionIndex - 1 >= 0 ? Segments[exceptionIndex - 1] : Segments[Segments.Count - 1];
#else           
                    Point p0 = GetReplacePoint(exceptionIndex, segments, consoleLogging);
#endif

                    ProgressManager.Instance.Progress.Log($"Update Point [{p0.X},{p0.Y}] to [{p0.X}, {segments[exceptionIndex].Y}]", consoleLogging);
                    p0.Y = segments[exceptionIndex].Y;
                    
                    retVal = segments;
            }
            break;
            case eArtifactExceptions.AE2:
            {
                    ProgressManager.Instance.Progress.Log($"Remove Point At: [{exceptionIndex + 1}]", consoleLogging);
                    segments.RemoveAt(exceptionIndex);
#if false
                    Point p0 = exceptionIndex - 1 >= 0 ? segments[exceptionIndex - 1] : segments[segments.Count - 1];
#else
                    Point p0 = GetReplacePoint(exceptionIndex, segments, consoleLogging);
#endif
                    ProgressManager.Instance.Progress.Log($"Update Point [{p0.X},{p0.Y}] to [{p0.X}, {segments[exceptionIndex].Y}]", consoleLogging);
                    p0.Y = segments[exceptionIndex].Y;
                    
                    retVal = segments;
            }
            break;
            case eArtifactExceptions.AE3:
                {
                    ProgressManager.Instance.Progress.Log($"Remove Point At: [{exceptionIndex + 1}]", consoleLogging);
                    segments.RemoveAt(exceptionIndex);
#if false
                    Point p0 = exceptionIndex - 1 >= 0 ? segments[exceptionIndex - 1] : segments[segments.Count - 1];
#else
                    Point p0 = GetReplacePoint(exceptionIndex, segments, consoleLogging);
#endif
                    ProgressManager.Instance.Progress.Log($"Update Point [{p0.X},{p0.Y}] to [{segments[exceptionIndex].X}, {p0.Y}]", consoleLogging);
                    p0.X = segments[exceptionIndex].X;
                    retVal = segments;
                }
                break;
            case eArtifactExceptions.AE4:
                {
                    ProgressManager.Instance.Progress.Log($"Remove Point At: [{exceptionIndex + 1}]", consoleLogging);
                    segments.RemoveAt(exceptionIndex);
#if false
                    Point p0 = exceptionIndex - 1 >= 0 ? segments[exceptionIndex - 1] : segments[segments.Count - 1];
#else
                    Point p0 = GetReplacePoint(exceptionIndex, segments, consoleLogging);
#endif
                    ProgressManager.Instance.Progress.Log($"Update Point [{p0.X},{p0.Y}] to [{segments[exceptionIndex].X}, {p0.Y}]", consoleLogging);
                    p0.X = segments[exceptionIndex].X;
                    retVal = segments;                    
                }
                break;
        }

        ProgressManager.Instance.Progress.Log($"^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", consoleLogging);

        return retVal;
    }

    static Point GetReplacePoint(int exceptionIndex, List<Point> Segments, eConsoleLogging consoleLogging)
    {
        Point retVal = null;
        
        int replacePoint = exceptionIndex - 1 > 0 ? exceptionIndex - 1 : Segments.Count - 1;
        retVal = Segments[replacePoint];

        ProgressManager.Instance.Progress.Log($"GetReplacePoint Returns: Index[{replacePoint + 1}] [{retVal}]", consoleLogging);
        return retVal;
    }
}
