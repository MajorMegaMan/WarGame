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

        DrawShape(target, points, target.lineColour, target.lineColour);

        DrawBoundary(target);

        DrawCookieCutterShape(target, points);


    }

    static void DrawLine(CookieCutter.CookieBox cookieBox, Vector2 start, Vector2 end, Color colour, Color notInsideColour)
    {
        CookieCutter.LineOverlap lineStatus = cookieBox.FindLineOverlap(start, end);
        if (lineStatus.lineIsInside)
        {
            Gizmos.color = colour;
        }
        else if(lineStatus.lineIsOverlapping)
        {
            Gizmos.color = notInsideColour;
        }
        else
        {
            Gizmos.color = Color.black;
        }

        Gizmos.DrawLine(start, end);

        Gizmos.color = Color.cyan;

        //List<Vector2> intersects = lineStatus.GetIntersections();
        List<Vector2> intersects = cookieBox.GetIntersections(lineStatus);
        if(intersects != null)
        {
            foreach (Vector2 point in intersects)
            {
                Gizmos.DrawSphere(point, 0.2f);
            }
        }

        if (cookieBox.ContainsPoint(start))
        {
            Gizmos.DrawCube(start, Vector3.one);
        }
    }

    static void DrawShape(DebugDrawShape target, List<Vector2> points, Color colour, Color notInsideColour)
    {
        CookieCutter.CookieBox cookieBox = new CookieCutter.CookieBox(target.mapWidth, target.mapHeight);

        Gizmos.color = colour;

        for (int i = 0; i < points.Count - 1; i++)
        {
            DrawLine(cookieBox, points[i], points[i + 1], colour, notInsideColour);
        }
        DrawLine(cookieBox, points[points.Count - 1], points[0], colour, notInsideColour);
    }

    static void DrawBoundary(DebugDrawShape target)
    {
        Gizmos.color = target.mapColour;

        float halfWidth = target.mapWidth / 2.0f;
        float halfHeight = target.mapHeight / 2.0f;

        CookieCutter.CookieBox cookieBox = new CookieCutter.CookieBox(target.mapWidth, target.mapHeight);

        void DrawPlane(Plane plane, float lineScale)
        {
            Vector3 tangent = plane.normal;
            tangent.x = plane.normal.y;
            tangent.y = -plane.normal.x;

            Vector3 position = plane.normal * -plane.distance;

            Gizmos.DrawLine(position + tangent * lineScale, position + tangent * -lineScale);

            // draw normal
            Gizmos.DrawLine(position, position + plane.normal);
        }

        DrawPlane(cookieBox.rightPlane, halfHeight);
        DrawPlane(cookieBox.leftPlane, halfHeight);
        DrawPlane(cookieBox.topPlane, halfWidth);
        DrawPlane(cookieBox.bottomPlane, halfWidth);

        //Vector2[] UV = new Vector2[4];
        //
        //UV[0] = new Vector2(-halfWidth, -halfHeight);
        //UV[1] = new Vector2(-halfWidth, halfHeight);
        //UV[2] = new Vector2(halfWidth, halfHeight);
        //UV[3] = new Vector2(halfWidth, -halfHeight);
        //
        //for (int i = 0; i < 3; i++)
        //{
        //    Gizmos.DrawLine(UV[i], UV[i + 1]);
        //}
        //Gizmos.DrawLine(UV[3], UV[0]);
    }

    static void DrawCookieCutterShape(DebugDrawShape target, List<Vector2> points)
    {
        List<Vector2> cookiePoints = new List<Vector2>(points);

        CookieCutter.CookieCutterShape(ref cookiePoints, target.mapWidth, target.mapHeight);

        DrawShape(target, cookiePoints, target.cookieShapeColour, Color.blue);

        for(int i = 0; i < points.Count; i++)
        {
            Handles.Label(points[i], i.ToString());
        }
    }
}