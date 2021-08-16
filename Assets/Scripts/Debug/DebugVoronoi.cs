using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi;
using Voronoi.Deluany;

using VShape = System.Collections.Generic.List<UnityEngine.Vector2>;

public class DebugVoronoi : MonoBehaviour
{
    List<Transform> points = null;
    List<DelTriangle> delTriangles;
    List<VoronoiPoint> vPoints;

    List<VShape> vShapes;

    public int shapeCount = 0;

    [Header("Deluanay")]
    public bool drawDelTriangles = true;
    public bool drawFirstTriCircle = true;
    public int triangleIndex = 0;

    [Header("Voronoi")]
    public bool drawVoronoiPoints = true;
    public bool drawVoronoiEdges = true;

    [Header("Bisectors")]
    public bool drawMidPoints = true;
    public bool drawPerpindicular = true;

    [Header("GiftWrapping")]
    public bool drawShapes = true;
    public bool drawVToCentre = true;
    public bool drawSingleShape = false;
    public int drawShapeIndex = 0;

    public List<VoronoiShape> debugShapes;

    void Awake()
    {
        InitPoints(out points);
        InitTriangles(out delTriangles, points);
        InitVoronoi(out vPoints, delTriangles);

        // Create deluany map
        VShape pointList = new VShape();
        foreach (var pointTransform in points)
        {
            pointList.Add(pointTransform.position);
        }
        DelaunyMap delaunyMap = new DelaunyMap(pointList);

        debugShapes = InitVShapes(delaunyMap, vPoints);
        shapeCount = debugShapes.Count;
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

    List<DelPoint> InitDelPoints(List<Transform> points)
    {
        List<DelPoint> delPoints = new List<DelPoint>();

        for (int i = 0; i < points.Count; i++)
        {
            delPoints.Add(new DelPoint(points[i].position, i));
        }
        return delPoints;
    }

    bool InitTriangles(out List<DelTriangle> delTris, List<Transform> points)
    {
        List<DelPoint> delPoints = InitDelPoints(points);
        return InitTriangles(out delTris, delPoints);
    }

    bool InitTriangles(out List<DelTriangle> delTris, List<DelPoint> delPoints)
    {
        if (delPoints.Count < 3)
        {
            // Not enoguh points to make a single triangle
            delTris = null;
            return false;
        }

        delTris = DelaunyMap.CalcTriangles(delPoints);
        return true;
    }

    void InitVoronoi(out List<VoronoiPoint> vPoints, List<DelTriangle> delTris)
    {
        vPoints = VoronoiDiagram.CreateVPoints(delTris);
    }

    List<VoronoiShape> InitVShapes(DelaunyMap delMap, List<VoronoiPoint> vPoints)
    {
        return VoronoiDiagram.FindShapes(vPoints, delMap);
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
            foreach(Connection connection in point.connectedPoints)
            {
                if(!connection.isEmpty)
                {
                    int index = connection.otherTriangle;
                    Gizmos.DrawLine(point.position, vPoints[index].position);
                }
            }
        }
    }

    void DrawMidPoints(List<VoronoiPoint> vPoints)
    {
        Gizmos.color = Color.cyan;
        foreach (var point in vPoints)
        {
            foreach (Connection connection in point.connectedPoints)
            {
                Gizmos.DrawSphere(connection.GetMidPoint(), 0.2f);
            }
        }
    }

    void DrawPerpindicularBisectors(List<VoronoiPoint> vPoints)
    {
        Gizmos.color = Color.cyan;
        foreach (var point in vPoints)
        {
            foreach (Connection connection in point.connectedPoints)
            {
                if(!connection.isEmpty)
                {
                    Vector2 midPoint = connection.GetMidPoint();
                    Gizmos.DrawLine(point.position, midPoint);
                }
                else
                {
                    Vector2 midPoint = connection.GetMidPoint();
                    Vector2 vPosition = vPoints[connection.owner].position;
                    Vector2 dir = (midPoint - vPosition).normalized;

                    if(!vPoints[connection.owner].isOutsideTri)
                    {
                        dir *= -1;
                    }

                    Vector2 targetLocation = vPosition + dir * 10.0f;
                    Gizmos.DrawLine(vPosition, targetLocation);
                }
            }
        }
    }

    void DrawShape(VoronoiShape shape, List<VoronoiPoint> vPoints)
    {
        Color shapeColour = Color.green;

        for (int i = 0; i < shape.points.Count - 1; i++)
        {
            Gizmos.color = shapeColour;
            Gizmos.DrawLine(vPoints[shape.points[i]].position, vPoints[shape.points[i + 1]].position);
        }
        Gizmos.color = shapeColour;
        Gizmos.DrawLine(vPoints[shape.points[shape.points.Count - 1]].position, vPoints[shape.points[0]].position);
    }

    void DrawVToCentre(VoronoiShape shape, List<VoronoiPoint> vPoints)
    {
        Color centreColour = Color.cyan;

        for (int i = 0; i < shape.points.Count - 1; i++)
        {
            Gizmos.color = centreColour;
            Gizmos.DrawLine(vPoints[shape.points[i]].position, shape.centre);
        }
        Gizmos.color = centreColour;
        Gizmos.DrawLine(vPoints[shape.points[shape.points.Count - 1]].position, shape.centre);
    }

    void OnDrawGizmos()
    {
        List<Transform> drawPoints = points;

        if(drawPoints == null || drawPoints.Count == 0)
        {
            InitPoints(out drawPoints);
        }

        // Create deluany map
        VShape pointList = new VShape();
        foreach(var pointTransform in drawPoints)
        {
            pointList.Add(pointTransform.position);
        }
        DelaunyMap delaunyMap = new DelaunyMap(pointList);

        if(drawDelTriangles)
        {
            DrawDelTris(delaunyMap.delTris);
        }
        if(drawFirstTriCircle)
        {
            DrawTriangleCircle(delaunyMap.delTris[triangleIndex].GetTriangle());
        }

            InitVoronoi(out List<VoronoiPoint> drawVPoints, delaunyMap.delTris);
        if(drawVoronoiPoints)
        {
            DrawVoronoiPoints(drawVPoints);
        }
        if(drawVoronoiEdges)
        {
            DrawVoronoiConnections(drawVPoints);
        }

        if(drawMidPoints)
        {
            DrawMidPoints(drawVPoints);
        }

        if(drawPerpindicular)
        {
            DrawPerpindicularBisectors(drawVPoints);
        }

        //if(!Application.isPlaying)
        //{
        //    return;
        //}

        var shapes = InitVShapes(delaunyMap, drawVPoints);

        if (drawShapes)
        {
            if(drawSingleShape)
            {
                var shape = shapes[drawShapeIndex];
                DrawShape(shape, drawVPoints);
            }
            else
            {
                foreach (var shape in shapes)
                {
                    DrawShape(shape, drawVPoints);
                }
            }
        }

        if (drawVToCentre)
        {
            if(drawSingleShape)
            {
                var shape = shapes[drawShapeIndex];
                DrawVToCentre(shape, drawVPoints);
            }
            else
            {
                foreach (var shape in shapes)
                {
                    DrawVToCentre(shape, drawVPoints);
                }
            }
        }
    }
}
