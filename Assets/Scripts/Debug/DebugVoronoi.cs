using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi;
using Voronoi.Deluany;

public class DebugVoronoi : MonoBehaviour
{
    List<Transform> points = null;
    List<DelTriangle> delTriangles;
    List<VoronoiPoint> vPoints;


    public bool drawDelTriangles = true;
    public bool drawFirstTriCircle = true;
    public bool drawVoronoi = true;

    void Awake()
    {
        InitPoints(out points);
        InitTriangles(out delTriangles, points);
        InitVoronoi(out vPoints, delTriangles);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitPoints(out List<Transform> points)
    {
        points = new List<Transform>();
        DebugCircle[] array = FindObjectsOfType<DebugCircle>();
        foreach (var circle in array)
        {
            points.Add(circle.transform);
        }
    }

    bool InitTriangles(out List<DelTriangle> delTris, List<Transform> points)
    {
        if (points.Count < 3)
        {
            // Not enoguh points to make a single triangle
            delTris = null;
            return false;
        }

        List<DelPoint> delPoints = new List<DelPoint>();

        for (int i = 0; i < points.Count; i++)
        {
            delPoints.Add(new DelPoint(points[i].position, i));
        }

        delTris = DelaunyMap.CalcTriangles(delPoints);
        return true;
    }

    void InitVoronoi(out List<VoronoiPoint> vPoints, List<DelTriangle> delTris)
    {
        vPoints = VoronoiDiagram.Create(delTris);
    }

    private void DrawDelTris(List<DelTriangle> delTris)
    {
        void DrawTri(Triangle tri)
        {
            Gizmos.DrawLine(tri.pointA, tri.pointB);
            Gizmos.DrawLine(tri.pointB, tri.pointC);
            Gizmos.DrawLine(tri.pointC, tri.pointA);
        }

        foreach (var delTri in delTris)
        {
            Triangle tempTri = delTri.GetTriangle();

            if (tempTri.FindWindingOrder())
            {
                // clockwise
                Gizmos.color = Color.red;
            }
            else
            {
                // counter clockwise
                Gizmos.color = Color.blue;
            }

            DrawTri(tempTri);
        }
    }

    void DrawTriangleCircle(Triangle tri)
    {
        Gizmos.color = Color.green;
        Vector3 circumCentre = tri.CalcCircumcentre();
        float radius = tri.CalcCircumcentreRadius();
        Gizmos.DrawWireSphere(circumCentre, radius);
    }

    void DrawVoronoiPoints(List<VoronoiPoint> vPoints)
    {
        Gizmos.color = Color.yellow;
        foreach(var point in vPoints)
        {
            Gizmos.DrawSphere(point.position, 0.2f);
        }
    }

    void DrawVoronoiConnections(List<VoronoiPoint> vPoints)
    {
        Gizmos.color = Color.yellow;
        foreach (var point in vPoints)
        {
            foreach(int index in point.connectedPoints)
            {
                Gizmos.DrawLine(point.position, vPoints[index].position);
            }
        }
    }

    void OnDrawGizmos()
    {
        List<Transform> drawPoints = points;

        if(drawPoints == null || drawPoints.Count == 0)
        {
            InitPoints(out drawPoints);
        }

        if(!InitTriangles(out List<DelTriangle> delTris, drawPoints))
        {
            return;
        }

        if(drawDelTriangles)
        {
            DrawDelTris(delTris);
        }
        if(drawFirstTriCircle)
        {
            DrawTriangleCircle(delTris[0].GetTriangle());
        }

        if(drawVoronoi)
        {
            InitVoronoi(out List<VoronoiPoint> drawVPoints, delTris);
            DrawVoronoiPoints(drawVPoints);
            DrawVoronoiConnections(drawVPoints);
        }
    }
}
