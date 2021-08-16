using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi.Deluany;

namespace Voronoi
{
    public class VoronoiDiagram
    {
        public List<VoronoiPoint> vPoints;
        public List<VoronoiShape> vShapes;
        public DelaunyMap delMap;

        public VoronoiDiagram(List<Vector2> points)
        {
            delMap = new DelaunyMap(points);
            vPoints = CreateVPoints(delMap);
            vShapes = FindShapes(vPoints, delMap);
        }

        public static List<VoronoiPoint> CreateVPoints(DelaunyMap delMap)
        {
            return CreateVPoints(delMap.delTris);
        }

        public static List<VoronoiPoint> CreateVPoints(List<Vector2> points)
        {
            List<DelPoint> delPoints = new List<DelPoint>();

            for (int i = 0; i < points.Count; i++)
            {
                delPoints.Add(new DelPoint(points[i], i));
            }
            return CreateVPoints(delPoints);
        }

        public static List<VoronoiPoint> CreateVPoints(List<DelPoint> delPoints)
        {
            List<DelTriangle> delTris = DelaunyMap.CalcTriangles(delPoints);
            return CreateVPoints(delTris);
        }

        public static List<VoronoiPoint> CreateVPoints(List<DelTriangle> delTris)
        {
            List<VoronoiPoint> points = new List<VoronoiPoint>();
            foreach (var tri in delTris)
            {
                VoronoiPoint vPoint = new VoronoiPoint(tri);
                points.Add(vPoint);
            }

            return points;
        }

        public static List<DelToVPoint> FindPointShapeList(List<VoronoiPoint> vPoints, DelaunyMap delMap)
        {
            List<DelToVPoint> result = new List<DelToVPoint>();
            foreach(DelPoint delPoint in delMap.delPoints)
            {
                DelToVPoint toAdd = new DelToVPoint();
                toAdd.delPoint = delPoint;
                toAdd.connectedVPoints = new List<VoronoiPoint>();
                result.Add(toAdd);
            }

            for(int i = 0; i < delMap.delTris.Count; i++)
            {
                foreach(var delToV in result)
                {
                    if(delToV.delPoint.IsInTriangle(delMap.delTris[i]))
                    {
                        delToV.connectedVPoints.Add(vPoints[i]);
                    }
                }
            }

            return result;
        }

        public static List<VoronoiShape> FindShapes(List<VoronoiPoint> vPoints, List<DelToVPoint> delToVPoints)
        {
            List<VoronoiShape> shapes = new List<VoronoiShape>();

            foreach (var delToV in delToVPoints)
            {
                if (delToV.connectedVPoints.Count < 3)
                {
                    continue;
                }

                VoronoiShape tempShape = new VoronoiShape(delToV);
                shapes.Add(tempShape);
            }

            foreach (var shape in shapes)
            {
                shape.GiftWrap(vPoints);
            }

            return shapes;
        }

        public static List<VoronoiShape> FindShapes(List<VoronoiPoint> vPoints, DelaunyMap delMap)
        {
            var delToVPoints = FindPointShapeList(vPoints, delMap);
            return FindShapes(vPoints, delToVPoints);
        }
    }

    public struct VoronoiPoint
    {
        public DelTriangle delTri;
        public Vector2 position;
        public bool isOutsideTri;
        public int index { get { return delTri.index; } }
        public List<Connection> connectedPoints { get { return delTri.connectedTris; } }

        public VoronoiPoint(DelTriangle delTri)
        {
            this.delTri = delTri;
            position = delTri.CalcCircumcentre();
            isOutsideTri = delTri.GetTriangle().ContainsPoint(position);
        }
    }

    public struct DelToVPoint
    {
        public DelPoint delPoint;
        public List<VoronoiPoint> connectedVPoints;
    }

    public class VoronoiShape
    {
        public Vector2 centre = Vector2.zero;
        public List<int> points = new List<int>();
        public List<int> otherConnected = new List<int>();

        public VoronoiShape(DelToVPoint delVPoint)
        {
            centre = delVPoint.delPoint.point;
            foreach (var vPoint in delVPoint.connectedVPoints)
            {
                points.Add(vPoint.index);
            }
        }

        public void GiftWrap(List<VoronoiPoint> vPoints)
        {
            // Check shape has enough points
            if (points.Count < 3)
            {
                Debug.LogError("This shape shouldn't exist. There is less than 3 vertices");
                return;
            }

            // Initialise hull
            List<int> hull = new List<int>();

            // Find Left most point
            int leftMost = 0; // index of points, not vPoints
            for (int i = 1; i < points.Count; i++)
            {
                if (vPoints[points[i]].position.x < vPoints[points[leftMost]].position.x)
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

        // Finds winding order or if they are co linear of any 3 given points.
        // 0 = p, q, r are co linear (a straight line)
        // 1 = ClockWise Triangle
        // 2 = Counter-clockWise Triangle
        static int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
            if (val == 0)
            {
                return 0; // co linear
            }
            return (val > 0) ? 1 : 2;
        }
    }
}
