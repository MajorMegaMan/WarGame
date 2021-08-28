using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi.Helpers;

public class DebugLineIntersection : MonoBehaviour
{
    [System.Serializable]
    public struct Line
    {
        public Transform start;
        public Transform end;

        public Vector3 direction { get { return end.position - start.position; } }
    }

    [System.Serializable]
    public struct PlaneMaker
    {
        public Transform point;
        public Transform toNormal;

        public Vector3 normal {  get { return (toNormal.position - point.position).normalized; } }
    }    

    public Line first;

    public PlaneMaker planeMaker;

    public float width = 10.0f;
    public float height = 10.0f;

    public float allowance = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Plane CreatePlane()
    {
        Plane plane = new Plane(planeMaker.normal, planeMaker.point.position);
        return plane;
    }

    int IsOverlappingPlane(Line line)
    {
        Plane plane = CreatePlane();

        return VMaths.IsOverlappingPlane(plane, line.start.position, line.end.position);
    }

    bool LinePlaneIntersection(out Vector2 intersect)
    {
        Plane plane = CreatePlane();
        return VMaths.LinePlaneIntersection(out intersect, first.start.position, first.direction.normalized, plane);
    }

    void DrawLine(Line line)
    {
        Gizmos.DrawLine(line.start.position, line.end.position);
    }

    void DrawCookieBox(CookieCutter.CookieBox cookieBox, Color colour)
    {
        // Draw CookieBox
        Gizmos.color = colour;

        float halfWidth = width / 2.0f;
        float halfHeight = height / 2.0f;

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
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawLine(first);

        Plane plane = CreatePlane();
        Vector3 planePos = plane.normal * -plane.distance;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(planePos, planePos + plane.normal);

        Vector3 tangent = planeMaker.normal;
        tangent.x = planeMaker.normal.y;
        tangent.y = -planeMaker.normal.x;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(planePos, planePos + tangent * 100.0f);
        Gizmos.DrawLine(planePos, planePos + tangent * -100.0f);

        if (LinePlaneIntersection(out Vector2 planeIntersect))
        {
            int side = IsOverlappingPlane(first);
            if (side == 1)
            {
                // safe side
                Gizmos.color = Color.cyan;
            }
            if (side == 0)
            {
                // overlapping
                Gizmos.color = Color.blue;
            }
            if (side == -1)
            {
                // past plane
                Gizmos.color = Color.red;
            }

            Gizmos.DrawSphere(planeIntersect, 0.4f);
        }

        CookieCutter.CookieBox cookieBox = new CookieCutter.CookieBox(width, height);

        DrawCookieBox(cookieBox, Color.blue);

        var lineStatus = cookieBox.FindLineOverlap(first.start.position, first.end.position);

        List<Vector2> intersects = cookieBox.GetIntersections(lineStatus);
        if(intersects != null)
        {
            Gizmos.color = Color.cyan;
            foreach(Vector2 point in intersects)
            {
                Gizmos.DrawSphere(point, 0.2f);
            }
        }
    }
}
