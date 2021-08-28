using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi;
using Voronoi.Deluany;

using PointList = System.Collections.Generic.List<UnityEngine.Vector2>;

public class DebugVoronoi : MonoBehaviour
{
    List<Transform> points = null;
    List<DelTriangle> delTriangles;
    List<VoronoiPoint> vPoints;

    List<PointList> vShapes;

    public int shapeCount = 0;

    [Header("Deluanay")]
    public float maxCircumCircleRadius = float.PositiveInfinity;
    public bool drawDelPoints = true;
    public bool drawTargetDelPoint = true;
    public int targetDelPoint = 0;
    public bool drawDelTriangles = true;
    public bool drawTargetDelTriangle = true;
    public bool drawTargetTriCircle = true;
    public int triangleIndex = 0;
    public bool drawMeanCentre = true;
    public bool drawDelPointConnections = false;

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
    public bool drawNeighbourConnections = false;
    public bool drawShapePointIndex = false;
    public int shapePointIndex = 0;

    public List<VoronoiShape> debugShapes;

    [Header("Boundary")]
    public int mapWidth = 10;
    public int mapHeight = 10;

    [Header("Random Values")]
    public bool useRandomPoints = false;
    public int seed = 0;

    public int pointcount = 10;

    void Awake()
    {
        InitPoints(out points);
        InitTriangles(out delTriangles, points);
        InitVoronoi(out vPoints, delTriangles);

        // Create deluany map
        PointList pointList = new PointList();
        foreach (var pointTransform in points)
        {
            pointList.Add(pointTransform.position);
        }
        DelaunyMap delaunyMap = new DelaunyMap(pointList, maxCircumCircleRadius);

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
        var vShapes = VoronoiDiagram.FindShapes(vPoints, delMap, debugBoundaryDist, mapWidth, mapHeight);
        VoronoiDiagram.FixVoronoiDiagramCorners(vShapes, mapWidth, mapHeight);
        return vShapes;
    }

    void DrawTri(Triangle tri)
    {
        Gizmos.DrawLine(tri.pointA, tri.pointB);
        Gizmos.DrawLine(tri.pointB, tri.pointC);
        Gizmos.DrawLine(tri.pointC, tri.pointA);
    }

    private void DrawDelTris(List<DelTriangle> delTris)
    {
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
                    Vector2 dir = connection.GetBiSector(vPoints[connection.owner]);

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

        if(drawShapePointIndex)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(shape.points[shapePointIndex], Vector3.one);
        }
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

    void DrawBoundary()
    {
        float halfWidth = (float)mapWidth / 2.0f;
        float halfHeight = (float)mapHeight / 2.0f;

        Vector2[] UV = new Vector2[4];

        UV[0] = new Vector2(-halfWidth, -halfHeight);
        UV[1] = new Vector2(-halfWidth, halfHeight);
        UV[2] = new Vector2(halfWidth, halfHeight);
        UV[3] = new Vector2(halfWidth, -halfHeight);


        Gizmos.color = Color.black;
        for (int i = 0; i < 3; i++)
        {
            Gizmos.DrawLine(UV[i], UV[i + 1]);
        }
        Gizmos.DrawLine(UV[3], UV[0]);
    }

    void OnDrawGizmos()
    {
        List<Transform> drawPoints = points;

        if(drawPoints == null || drawPoints.Count == 0)
        {
            InitPoints(out drawPoints);
        }

        // DrawBoundary
        DrawBoundary();

        // Create deluany map
        PointList pointList = new PointList();

        if(!useRandomPoints)
        {
            foreach (var pointTransform in drawPoints)
            {
                pointList.Add(pointTransform.position);
            }
        }
        else
        {
            Random.InitState(seed);

            float halfWidth = (float)mapWidth / 2.0f;
            float halfHeight = (float)mapHeight / 2.0f;

            for (int i = 0; i < pointcount; i++)
            {
                Vector2 newPoint = Vector2.zero;

                newPoint.x = Random.Range(-halfWidth, halfWidth);
                newPoint.y = Random.Range(-halfHeight, halfHeight);

                pointList.Add(newPoint);
            }
        }

        if(drawDelPoints)
        {
            Gizmos.color = Color.red;
            foreach(var point in pointList)
            {
                Gizmos.DrawSphere(point, 0.4f);
            }
        }

        DelaunyMap delaunyMap = new DelaunyMap(pointList, maxCircumCircleRadius);

        if(drawTargetDelPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(delaunyMap.delPoints[targetDelPoint].point, 0.5f);
        }

        if(drawDelTriangles)
        {
            DrawDelTris(delaunyMap.delTris);
        }

        if (drawTargetDelTriangle)
        {
            Gizmos.color = Color.yellow;
            DrawTri(delaunyMap.delTris[triangleIndex].GetTriangle());
        }

        if (drawTargetTriCircle)
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

        if(drawDelPointConnections)
        {
            Gizmos.color = Color.red;
            if(drawTargetDelPoint)
            {
                DelPoint delPoint = delaunyMap.delPoints[targetDelPoint];
                foreach (DelPoint targetConnect in delPoint.connectedDelPoints)
                {
                    Gizmos.DrawLine(delPoint.point, targetConnect.point);
                }
            }
            else
            {
                foreach(DelPoint delPoint in delaunyMap.delPoints)
                {
                    foreach(DelPoint targetConnect in delPoint.connectedDelPoints)
                    {
                        Gizmos.DrawLine(delPoint.point, targetConnect.point);
                    }
                }
            }
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

            if (drawSingleVPoint)
            {
                Gizmos.color = Color.cyan;
                var point = drawVPoints[vPointIndex];
                {
                    foreach (Connection connection in point.connectedPoints)
                    {
                        if (!connection.isEmpty)
                        {
                            int index = connection.otherTriangle;
                            Gizmos.DrawLine(point.position, drawVPoints[index].position);
                        }
                    }
                }
            }
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

        if (drawNeighbourConnections)
        {
            Gizmos.color = Color.red;
            if (drawSingleShape)
            {
                var shape = shapes[drawShapeIndex];
                foreach (int targetConnect in shape.neighbours)
                {
                    Gizmos.DrawLine(shape.centre, shapes[targetConnect].centre);
                }
            }
            else
            {
                foreach (var shape in shapes)
                {
                    foreach (int targetConnect in shape.neighbours)
                    {
                        Gizmos.DrawLine(shape.centre, shapes[targetConnect].centre);
                    }
                }
            }
        }
    }

    private void OnValidate()
    {
        if(useRandomPoints)
        {
            if (mapWidth < 1)
            {
                mapWidth = 1;
            }
            if (mapHeight < 1)
            {
                mapHeight = 1;
            }

            if (pointcount < 3)
            {
                pointcount = 3;
            }
        }
        if(debugBoundaryDist <= 0)
        {
            debugBoundaryDist = 0.0001f;
        }
        if(maxCircumCircleRadius <= 0)
        {
            maxCircumCircleRadius = 0.0001f;
        }
    }
}
