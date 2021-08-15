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

    public bool drawDelTriangles = true;
    public bool drawFirstTriCircle = true;
    public bool drawVoronoiPoints = true;
    public bool drawVoronoiEdges = true;
    public bool drawShapes = true;
    public bool drawVToCentre = true;
    public bool drawSingleShape = false;
    public int drawShapeIndex = 0;

    public List<TempShape> debugShapes;

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

    public class TempShape
    {
        public Vector2 centre = Vector2.zero;
        public List<int> points = new List<int>();
        public List<int> otherConnected = new List<int>();

        public void GiftWrap(List<VoronoiPoint> vPoints)
        {
            // Check shape has enough points
            if(points.Count < 3)
            {
                Debug.LogError("This shape shouldn't exist. There is less than 3 vertices");
                return;
            }

            // Initialise hull
            List<int> hull = new List<int>();

            // Find Left most point
            int leftMost = 0; // index of points, not vPoints
            for(int i = 1; i < points.Count; i++)
            {
                if(vPoints[points[i]].position.x < vPoints[points[leftMost]].position.x)
                {
                    leftMost = i;
                }
            }

            // start from leftmost, keep moving counter clockwise until reach start position again
            int p = leftMost;
            int q = 0;
            do
            {
                // Add current to hull result
                hull.Add(points[p]);

                // Search for a point 'q' such that orientation(p, x,
                // q) is counterclockwise for all points 'x'. The idea
                // is to keep track of last visited most counterclock-
                // wise point in q. If any point 'i' is more counterclock-
                // wise than q, then update q.
                q = (p + 1) % points.Count;
                for (int i = 0; i < points.Count; i++)
                {
                    // If i is more counterclockwise than current q, then
                    // update q
                    if (Orientation(vPoints[points[p]].position, vPoints[points[i]].position, vPoints[points[q]].position) == 2)
                    {
                        q = i;
                    }
                }

                // Now q is the most counterclockwise with respect to p
                // Set p as q for next iteration, so that q is added to
                // result 'hull'
                p = q;
            } while (p != leftMost);

            // Finish result
            points = hull;
        }
    }

    // Finds winding order or if they are co linear of any 3 given points.
    // 0 = p, q, r are co linear (a straight line)
    // 1 = ClockWise Triangle
    // 2 = Counter-clockWise Triangle
    static int Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
        if(val == 0)
        {
            return 0; // co linear
        }
        return (val > 0) ? 1 : 2;
    }

    class TempDelPointTris
    {
        public int pointIndex = 0;
        public List<int> tris = new List<int>();

        public TempDelPointTris(int pointIndex)
        {
            this.pointIndex = pointIndex;
        }
    }

    List<TempShape> InitVShapes(DelaunyMap delMap, List<VoronoiPoint> vPoints)
    {
        // would rather do a* to search back to the start using as the crow flies but being lazy right now.

        List<DelPoint> delPoints = delMap.delPoints;
        List<DelTriangle> delTris = delMap.delTris;

        List<TempShape> shapes = new List<TempShape>();

        var delToVPoints = VoronoiDiagram.FindShapes(vPoints, delMap);

        foreach(var delToV in delToVPoints)
        {
            if(delToV.connectedVPoints.Count < 3)
            {
                continue;
            }

            TempShape tempShape = new TempShape();
            tempShape.centre = delToV.delPoint.point;
            foreach(var vPoint in delToV.connectedVPoints)
            {
                tempShape.points.Add(vPoint.index);
            }
            shapes.Add(tempShape);
        }

        foreach(var shape in shapes)
        {
            shape.GiftWrap(vPoints);
        }

        return shapes;
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
            DrawTriangleCircle(delaunyMap.delTris[0].GetTriangle());
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

        //if(!Application.isPlaying)
        //{
        //    return;
        //}

        var shapes = InitVShapes(delaunyMap, drawVPoints);

        if (drawShapes)
        {
            Color shapeColour = Color.green;

            if(drawSingleShape)
            {
                var shape = shapes[drawShapeIndex];
                {
                    for (int i = 0; i < shape.points.Count - 1; i++)
                    {
                        Gizmos.color = shapeColour;
                        Gizmos.DrawLine(drawVPoints[shape.points[i]].position, drawVPoints[shape.points[i + 1]].position);
                    }
                    Gizmos.color = shapeColour;
                    Gizmos.DrawLine(drawVPoints[shape.points[shape.points.Count - 1]].position, drawVPoints[shape.points[0]].position);
                }
            }
            else
            {
                foreach (var shape in shapes)
                {
                    for (int i = 0; i < shape.points.Count - 1; i++)
                    {
                        Gizmos.color = shapeColour;
                        Gizmos.DrawLine(drawVPoints[shape.points[i]].position, drawVPoints[shape.points[i + 1]].position);
                    }
                    Gizmos.color = shapeColour;
                    Gizmos.DrawLine(drawVPoints[shape.points[shape.points.Count - 1]].position, drawVPoints[shape.points[0]].position);
                }
            }
        }

        if (drawVToCentre)
        {
            Color centreColour = Color.cyan;

            if(drawSingleShape)
            {
                var shape = shapes[drawShapeIndex];
                {
                    for (int i = 0; i < shape.points.Count - 1; i++)
                    {
                        Gizmos.color = centreColour;
                        Gizmos.DrawLine(drawVPoints[shape.points[i]].position, shape.centre);
                    }
                    Gizmos.color = centreColour;
                    Gizmos.DrawLine(drawVPoints[shape.points[shape.points.Count - 1]].position, shape.centre);
                }
            }
            else
            {
                foreach (var shape in shapes)
                {
                    for (int i = 0; i < shape.points.Count - 1; i++)
                    {
                        Gizmos.color = centreColour;
                        Gizmos.DrawLine(drawVPoints[shape.points[i]].position, shape.centre);
                    }
                    Gizmos.color = centreColour;
                    Gizmos.DrawLine(drawVPoints[shape.points[shape.points.Count - 1]].position, shape.centre);
                }
            }
        }
    }
}
