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
    public bool drawTargetDelPoint = true;
    public int targetDelPoint = 0;
    public bool drawDelTriangles = true;
    public bool drawTargetTriCircle = true;
    public int triangleIndex = 0;
    public bool drawMeanCentre = true;

    [Header("Voronoi")]
    public bool drawVoronoiPoints = true;
    public bool drawVoronoiEdges = true;
    public float debugBoundaryDist = 10.0f;
    public bool drawSingleVPoint = false;
    public int vPointIndex = 0;

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
        return VoronoiDiagram.FindShapes(vPoints, delMap, debugBoundaryDist);
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
                    Vector2 vPosition = vPoints[connection.owner].position;
                    Vector2 triMeanAverage = vPoints[connection.owner].delTri.GetTriangle().FindMeanAverage();
                    Vector2 dir = connection.GetBiSector(vPosition, triMeanAverage);

                    Vector2 targetLocation = vPosition + dir * debugBoundaryDist;
                    Gizmos.DrawLine(vPosition, targetLocation);
                }
            }
        }
    }

    void DrawShape(VoronoiShape shape)
    {
        Color shapeColour = Color.green;

        for (int i = 0; i < shape.points.Count - 1; i++)
        {
            Gizmos.color = shapeColour;
            Gizmos.DrawLine(shape.points[i], shape.points[i + 1]);
        }
        Gizmos.color = shapeColour;
        Gizmos.DrawLine(shape.points[shape.points.Count - 1], shape.points[0]);
    }

    void DrawVToCentre(VoronoiShape shape)
    {
        Color centreColour = Color.cyan;

        for (int i = 0; i < shape.points.Count - 1; i++)
        {
            Gizmos.color = centreColour;
            Gizmos.DrawLine(shape.points[i], shape.centre);
        }
        Gizmos.color = centreColour;
        Gizmos.DrawLine(shape.points[shape.points.Count - 1], shape.centre);
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

        if(drawTargetDelPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(delaunyMap.delPoints[targetDelPoint].point, 0.5f);
        }

        if(drawDelTriangles)
        {
            DrawDelTris(delaunyMap.delTris);
        }
        if(drawTargetTriCircle)
        {
            DrawTriangleCircle(delaunyMap.delTris[triangleIndex].GetTriangle());
        }

        Vector2 meanCentre = Vector2.zero;
        foreach (DelPoint point in delaunyMap.delPoints)
        {
            meanCentre += point.point;
        }
        meanCentre /= delaunyMap.delPoints.Count;

        if (drawMeanCentre)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(meanCentre, 0.5f);
        }

        InitVoronoi(out List<VoronoiPoint> drawVPoints, delaunyMap.delTris);
        if(drawVoronoiPoints)
        {
            DrawVoronoiPoints(drawVPoints);

            if(drawSingleVPoint)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(drawVPoints[vPointIndex].position, 0.4f);
            }
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
                DrawShape(shape);
            }
            else
            {
                foreach (var shape in shapes)
                {
                    DrawShape(shape);
                }
            }
        }

        if (drawVToCentre)
        {
            if(drawSingleShape)
            {
                var shape = shapes[drawShapeIndex];
                DrawVToCentre(shape);
            }
            else
            {
                foreach (var shape in shapes)
                {
                    DrawVToCentre(shape);
                }
            }
        }
    }
}
