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

        public VoronoiDiagram(List<Vector2> points, float debugBoundaryDist)
        {
            delMap = new DelaunyMap(points);

            vPoints = CreateVPoints(delMap);
            vShapes = FindShapes(vPoints, delMap, debugBoundaryDist);
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

        public static List<VoronoiShape> FindShapes(List<DelToVPoint> delToVPoints, float debugBoundaryDist)
        {
            List<VoronoiShape> shapes = new List<VoronoiShape>();

            foreach (var delToV in delToVPoints)
            {
                VoronoiShape tempShape = new VoronoiShape(delToV, debugBoundaryDist);
                shapes.Add(tempShape);
            }

            return shapes;
        }

        public static List<VoronoiShape> FindShapes(List<VoronoiPoint> vPoints, DelaunyMap delMap, float debugBoundaryDist)
        {
            var delToVPoints = FindPointShapeList(vPoints, delMap);
            return FindShapes(delToVPoints, debugBoundaryDist);
        }
    }

    public struct VoronoiPoint
    {
        public DelTriangle delTri;
        public Vector2 position;
        public bool isOutsideTri;
        public int index { get { return delTri.index; } }
        public List<Connection> connectedPoints { get { return delTri.connectedTris; } }
        public bool hasEmptyConnection
        {
            get
            {
                foreach(Connection connect in connectedPoints)
                {
                    if(connect.isEmpty)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public VoronoiPoint(DelTriangle delTri)
        {
            this.delTri = delTri;
            position = delTri.CalcCircumcentre();
            isOutsideTri = delTri.GetTriangle().ContainsPoint(position);
        }

        public bool IsNeighbour(int otherVPointIndex)
        {
            foreach(Connection connection in delTri.connectedTris)
            {
                if(connection.otherTriangle == otherVPointIndex)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public struct DelToVPoint
    {
        public DelPoint delPoint;
        public List<VoronoiPoint> connectedVPoints;
    }

    public class VoronoiShape
    {
        int debugIndex = 0;
        public Vector2 centre = Vector2.zero;
        public List<Vector2> points = new List<Vector2>();
        public List<int> neighbours = new List<int>();

        public VoronoiShape(DelToVPoint delVPoint, float debugBoundaryDist)
        {
            debugIndex = delVPoint.delPoint.index;
            centre = delVPoint.delPoint.point;

            foreach(DelPoint neighbour in delVPoint.delPoint.connectedDelPoints)
            {
                neighbours.Add(neighbour.index);
            }

            int hasEmptyConnectionCount = 0;
            foreach (VoronoiPoint vPoint in delVPoint.connectedVPoints)
            {
                points.Add(vPoint.position);
                if(vPoint.hasEmptyConnection)
                {
                    hasEmptyConnectionCount++;
                }
            }

            FindPotentialBoundaryPoints(delVPoint, debugBoundaryDist);

            GiftWrap();
        }

        void FindPotentialBoundaryPoints(DelToVPoint delVPoint, float debugBoundaryDist)
        {
            if(delVPoint.connectedVPoints.Count == 1)
            {
                VoronoiPoint vPoint = delVPoint.connectedVPoints[0];
                List<Connection> emptyConnections = new List<Connection>();
                foreach (Connection connect in vPoint.connectedPoints)
                {
                    if (connect.isEmpty)
                    {
                        emptyConnections.Add(connect);
                    }
                }

                if(emptyConnections.Count == 2)
                {
                    foreach(Connection emptyConnect in emptyConnections)
                    {
                        FindBoundaryPoint(emptyConnect, vPoint, debugBoundaryDist);
                    }
                }
                else if(emptyConnections.Count == 3)
                {
                    // This is the only vPoint in the diagram

                    // need to 2 find connections that point towards outside
                    foreach(Connection connect in emptyConnections)
                    {
                        float connectDot = GetDotCompareValue(connect, vPoint, delVPoint);
                        // if the dot product between 
                        if(connectDot < 0)
                        {
                            FindBoundaryPoint(connect, vPoint, debugBoundaryDist);
                        }
                    }
                }
            }
            else if (delVPoint.connectedVPoints.Count < 3)
            {
                // only two connections therefore it must be on the outside
                foreach (VoronoiPoint vPoint in delVPoint.connectedVPoints)
                {
                    // Get Count of empty Connections
                    List<Connection> emptyConnections = new List<Connection>();
                    foreach (Connection connect in vPoint.connectedPoints)
                    {
                        if(connect.isEmpty)
                        {
                            emptyConnections.Add(connect);
                        }
                    }

                    if(emptyConnections.Count > 1)
                    {
                        // need to find connection that points towards outside
                        Connection target = emptyConnections[0];
                        float lowestDot = GetDotCompareValue(target, vPoint, delVPoint);

                        for (int i = 1; i < emptyConnections.Count; i++)
                        {
                            Connection currentConnection = emptyConnections[i];
                            float currentDot = GetDotCompareValue(currentConnection, vPoint, delVPoint);
                            if(currentDot < lowestDot)
                            {
                                target = currentConnection;
                                lowestDot = currentDot;
                            }
                        }

                        FindBoundaryPoint(target, vPoint, debugBoundaryDist);
                    }
                    else
                    {
                        // safe to use the empty connection
                        FindBoundaryPoint(emptyConnections[0], vPoint, debugBoundaryDist);
                    }
                }
            }
            else
            {
                // Get vPoints with empty connections
                List<VoronoiPoint> emptyPoints = new List<VoronoiPoint>();

                foreach (VoronoiPoint vPoint in delVPoint.connectedVPoints)
                {
                    int vPointEmptyCount = 0;
                    foreach (Connection connect in vPoint.connectedPoints)
                    {
                        if(connect.isEmpty)
                        {
                            vPointEmptyCount++;
                        }
                    }

                    switch (vPointEmptyCount)
                    {
                        case 0:
                            {
                                // This point does not lie on the outside of the voronoi pattern
                                break;
                            }
                        case 1:
                            {
                                // This Point does lie on the outside of the voronoi pattern
                                emptyPoints.Add(vPoint);
                                break;
                            }
                        case 2:
                            {
                                // This Point is connected to 2 deluanay triangles and will there for be used in 2 outside shapes
                                emptyPoints.Add(vPoint);
                                break;
                            }
                        case 3:
                            {
                                // This is the only Voronoi Point in the diagram
                                Debug.LogWarning("Shouldn't be here, this is already checked.");
                                emptyPoints.Add(vPoint);
                                break;
                            }
                    }
                }

                // Shapes on the outside can only ever have 2 V points with empty Connections
                if(emptyPoints.Count == 2)
                {
                    // Check if emptyPoints are not neighbours with each other
                    if (!emptyPoints[0].IsNeighbour(emptyPoints[1].index))
                    {
                        // They are not neighbour which means we can extend the shape outwards as it lies on the outside of the Voronoi pattern
                        foreach (var vPoint in emptyPoints)
                        {
                            foreach (Connection connect in vPoint.connectedPoints)
                            {
                                FindBoundaryPoint(connect, vPoint, debugBoundaryDist);
                            }
                        }
                    }
                }
            }
        }

        void FindBoundaryPoint(Connection connection, VoronoiPoint vPoint, float debugBoundaryDist)
        {
            if(!connection.isEmpty)
            {
                // Don't need to bother find a point as this connection leads to another voronoi point
                return;
            }

            Vector2 vPosition = vPoint.position;
            Vector2 triMeanAverage = vPoint.delTri.GetTriangle().FindMeanAverage();
            Vector2 dir = connection.GetBiSector(vPosition, triMeanAverage);

            points.Add(vPosition + dir * debugBoundaryDist);
        }

        float GetDotCompareValue(Connection target, VoronoiPoint vPoint, DelToVPoint delVPoint)
        {
            Vector2 triMeanAverage = vPoint.delTri.GetTriangle().FindMeanAverage();
            Vector2 toTriMean = (triMeanAverage - delVPoint.delPoint.point).normalized;

            Vector2 vPosition = vPoint.position;

            Vector2 vBiSectorDir = target.GetBiSector(vPosition, triMeanAverage);

            return Vector2.Dot(toTriMean, vBiSectorDir);
        }

        void GiftWrap()
        {
            // Check shape has enough points
            if (points.Count < 3)
            {
                Debug.LogError("DelPoint: " + debugIndex + " | This shape shouldn't exist. There is less than 3 vertices");
                return;
            }

            // Initialise hull
            List<Vector2> hull = new List<Vector2>();

            // Find Left most point
            int leftMost = 0; // index of points, not vPoints
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].x < points[leftMost].x)
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
                    if (Orientation(points[p], points[i], points[q]) == 2)
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
