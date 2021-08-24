using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Voronoi.Helpers;

[CustomEditor(typeof(DebugDrawShape))]
public class DebugShapeEditor : Editor
{
    static Color m_toBoundaryNormalLineColour = Color.blue;
    static Color m_offsetColour = Color.black;
    static Color m_toBoundaryOffsetLinecolour = Color.yellow;

    static CookieCutter.BoundaryBreakIndices m_breakOutIndices;

    public override void OnInspectorGUI()
    {
        DebugDrawShape targetInspect = target as DebugDrawShape;

        // if true, a value was changed
        if (DrawDefaultInspector())
        {
            // Don't know what to do with this yet
            Debug.Log("Value changed");
        }
        m_toBoundaryNormalLineColour = EditorGUILayout.ColorField("To Boundary Normal Colour", m_toBoundaryNormalLineColour);
        m_offsetColour = EditorGUILayout.ColorField("Offset Colour", m_offsetColour);
        m_toBoundaryOffsetLinecolour = EditorGUILayout.ColorField("To Boundary Offset Colour", m_toBoundaryOffsetLinecolour);

        GUILayout.Label("Boundary BreakIndices", EditorStyles.boldLabel);
        IntLabel("Enter", m_breakOutIndices.enter);
        IntLabel("After Enter", m_breakOutIndices.afterEnter);
        IntLabel("Exit", m_breakOutIndices.exit);
        IntLabel("Before Exit", m_breakOutIndices.beforeExit);
    }

    void IntLabel(string label, int value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(100.0f));
        GUILayout.Label(value.ToString());
        GUILayout.EndHorizontal();
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NotInSelectionHierarchy)]
    static void DrawGizmosForDebugShape(DebugDrawShape target, GizmoType gizmoType)
    {
        List<Vector2> points = new List<Vector2>();

        var array = FindObjectsOfType<DebugCircle>();

        foreach (var debugCircle in array)
        {
            points.Add(debugCircle.transform.position);
        }

        VMaths.GiftWrap(ref points);

        DrawShape(points, target.lineColour);

        DrawBoundary(target);

        DrawCookieCutterShape(target, points);

        
    }

    static void DrawShape(List<Vector2> points, Color colour)
    {
        Gizmos.color = colour;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
        Gizmos.DrawLine(points[points.Count - 1], points[0]);
    }

    static void DrawBoundary(DebugDrawShape target)
    {
        Gizmos.color = target.mapColour;

        float halfwidth = target.mapWidth / 2.0f;
        float halfHeight = target.mapHeight / 2.0f;

        Vector2[] UV = new Vector2[4];

        UV[0] = new Vector2(-halfwidth, -halfHeight);
        UV[1] = new Vector2(-halfwidth, halfHeight);
        UV[2] = new Vector2(halfwidth, halfHeight);
        UV[3] = new Vector2(halfwidth, -halfHeight);

        for (int i = 0; i < 3; i++)
        {
            Gizmos.DrawLine(UV[i], UV[i + 1]);
        }
        Gizmos.DrawLine(UV[3], UV[0]);
    }

    static void DrawCookieCutterShape(DebugDrawShape target, List<Vector2> points)
    {
        List<Vector2> cookiePoints = new List<Vector2>(points);

        CookieCutter.CookieCutterShape(ref cookiePoints, target.mapWidth, target.mapHeight);

        DrawShape(cookiePoints, target.cookieShapeColour);


        List<Vector2> stepCookiePoints = new List<Vector2>(points);
        CookieSteps(ref stepCookiePoints, target.mapWidth, target.mapHeight);

        for(int i = 0; i < points.Count; i++)
        {
            Handles.Label(points[i], i.ToString());
        }
    }

    public static void CookieSteps(ref List<Vector2> points, float width, float height)
    {
        // Find all points outside bounds
        float halfWidth = width / 2.0f;
        float halfHeight = height / 2.0f;

        // Search all points in shape to find points that may lie outside the bounds
        CookieCutter.ShapeExtrudingPoints extrudingPoints = new CookieCutter.ShapeExtrudingPoints();
        List<int> safePoints = new List<int>();

        for (int i = 0; i < points.Count; i++)
        {
            CookieCutter.BoundsDir boundsDir = PointOutSideBounds(points[i], halfWidth, halfHeight);
            if (boundsDir != CookieCutter.BoundsDir.inside)
            {
                // point is outside shape
                extrudingPoints.pointIndices.Add(i);
                extrudingPoints.pointDir.Add(boundsDir);
            }
            else
            {
                safePoints.Add(i);
            }
        }


        if (extrudingPoints.extrudingCount > 0)
        {
            // Has points outside
            if (extrudingPoints.extrudingCount == 1)
            {
                // Shape is not on the outside of the voronoi pattern but is should be clamped
                // Add in an extra position as two lines will cross the boundary

            }
            else
            {
                // Shape has many points out of bounds which means only the first and last need to be clamped, the rest can be deleted
                // clamp first to not confuse the indices list, delete later
                // use first point index - 1 and last point index + 1 to find directions
                CookieCutter.BoundaryBreakIndices breakOutIndices = FindBoundaryBreakIndices(extrudingPoints, points.Count);
                m_breakOutIndices = breakOutIndices;

                int exitIndex = breakOutIndices.exit;
                int enterIndex = breakOutIndices.enter;

                int beforeExit = breakOutIndices.beforeExit;
                int afterEnter = breakOutIndices.afterEnter;

                Vector2 firstResult = CalcOffsetToBoundary(points[exitIndex], points[beforeExit], extrudingPoints.pointDir[0], halfWidth, halfHeight);

                Vector2 lastResult = CalcOffsetToBoundary(points[enterIndex], points[afterEnter], extrudingPoints.pointDir[extrudingPoints.extrudingCount - 1], halfWidth, halfHeight);

                points[exitIndex] += firstResult;
                points[enterIndex] += lastResult;

                // delete middle points
                safePoints.Insert(0, exitIndex);
                safePoints.Add(enterIndex);

                RemoveBoundaryPoints(ref points, safePoints);
            }
        }

        // Use found points

        // Check if points should be 
    }

    public static Vector2 ClampPoint(Vector2 pointToClamp, CookieCutter.BoundsDir boundsDirFlag, float halfWidth, float halfHeight)
    {
        Vector2 clampResult = CookieCutter.ClampPoint(pointToClamp, boundsDirFlag, halfWidth, halfHeight);
        return clampResult;
    }

    public static Vector2 CalcOffsetToBoundary(Vector2 start, Vector2 end, CookieCutter.BoundsDir startBoundsFlag, float halfWidth, float halfHeight)
    {
        Vector2 direction = end - start;
        Vector2 clampedPoint = ClampPoint(start, startBoundsFlag, halfWidth, halfHeight);
        Vector2 pointToBoundary = clampedPoint - start;
        Vector2 result = VMaths.ProjectVector(pointToBoundary, direction);

        Gizmos.color = m_toBoundaryNormalLineColour;
        Gizmos.DrawLine(start, clampedPoint);

        Gizmos.color = m_offsetColour;
        Gizmos.DrawLine(start, end);

        //Gizmos.color = m_toBoundaryOffsetLinecolour;
        //Gizmos.DrawLine(start, start + result);

        result = CookieCutter.CalcOffsetToBoundary(start, end, startBoundsFlag, halfWidth, halfHeight);
        return result;
    }

    public static CookieCutter.BoundsDir PointOutSideBounds(Vector2 point, float halfWidth, float halfHeight)
    {
        CookieCutter.BoundsDir boundsDir = CookieCutter.PointOutSideBounds(point, halfWidth, halfHeight);
        return boundsDir;
    }

    public static CookieCutter.BoundaryBreakIndices FindBoundaryBreakIndices(CookieCutter.ShapeExtrudingPoints extrudingPoints, int pointsCount)
    {
        CookieCutter.BoundaryBreakIndices indices = CookieCutter.FindBoundaryBreakIndices(extrudingPoints, pointsCount);
        return indices;
    }

    public static void RemoveBoundaryPoints(ref List<Vector2> points, List<int> safePoints)
    {
        CookieCutter.RemoveBoundaryPoints(ref points, safePoints);
    }
}