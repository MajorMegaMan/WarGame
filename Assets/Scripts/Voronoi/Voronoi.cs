using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi.Deluany;
using Voronoi.Helpers;

namespace Voronoi
{
    public class VoronoiDiagram
    {
        public List<VoronoiPoint> vPoints;
        public List<VoronoiShape> vShapes;
        public DelaunyMap delMap;

        float m_boundaryWidth;
        float m_boundaryHeight;
        float m_debugBoundaryDist;

        public VoronoiDiagram(List<Vector2> points, float debugBoundaryDist, float boundaryWidth, float boundaryHeight)
        {
            Init(points, debugBoundaryDist, boundaryWidth, boundaryHeight);
        }

        public VoronoiDiagram(List<Vector2> points, float debugBoundaryDist, float boundaryWidth, float boundaryHeight, float maxRadius)
        {
            Init(points, debugBoundaryDist, boundaryWidth, boundaryHeight, maxRadius);
        }

        void Init(List<Vector2> points, float debugBoundaryDist, float boundaryWidth, float boundaryHeight)
        {
            InitVar(boundaryWidth, boundaryHeight, debugBoundaryDist);
            InitDeluanay(points);
            InitVoronoi(debugBoundaryDist, boundaryWidth, boundaryHeight);
        }

        void Init(List<Vector2> points, float debugBoundaryDist, float boundaryWidth, float boundaryHeight, float maxRadius)
        {
            InitVar(boundaryWidth, boundaryHeight, debugBoundaryDist);
            InitDeluanay(points, maxRadius);
            InitVoronoi(debugBoundaryDist, boundaryWidth, boundaryHeight);
        }
        
        void InitVar(float boundaryWidth, float boundaryHeight, float debugBoundaryDist)
        {
            m_boundaryWidth = boundaryWidth;
            m_boundaryHeight = boundaryHeight;
            m_debugBoundaryDist = debugBoundaryDist;
        }

        void InitDeluanay(List<Vector2> points)
        {
            delMap = new DelaunyMap(points);
        }

        void InitDeluanay(List<Vector2> points, float maxRadius)
        {
            delMap = new DelaunyMap(points, maxRadius);
        }

        void InitVoronoi(float debugBoundaryDist, float boundaryWidth, float boundaryHeight)
        {
            vPoints = CreateVPoints(delMap);
            vShapes = FindShapes(vPoints, delMap, debugBoundaryDist, boundaryWidth, boundaryHeight);

            FixVoronoiDiagramCorners(vShapes, boundaryWidth, boundaryHeight);
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

        public static List<VoronoiShape> FindShapes(List<DelToVPoint> delToVPoints, float debugBoundaryDist, float boundaryWidth, float boundaryHeight)
        {
            List<VoronoiShape> shapes = new List<VoronoiShape>();

            foreach (var delToV in delToVPoints)
            {
                VoronoiShape tempShape = new VoronoiShape(delToV, debugBoundaryDist, boundaryWidth, boundaryHeight);
                shapes.Add(tempShape);
            }

            return shapes;
        }

        public static List<VoronoiShape> FindShapes(List<VoronoiPoint> vPoints, DelaunyMap delMap, float debugBoundaryDist, float boundaryWidth, float boundaryHeight)
        {
            var delToVPoints = FindPointShapeList(vPoints, delMap);
            return FindShapes(delToVPoints, debugBoundaryDist, boundaryWidth, boundaryHeight);
        }

        struct SearchValues
        {
            public VoronoiShape shape;
            public int index;
            public Vector2 point;

            public SearchValues(VoronoiShape shape, int index, Vector2 point)
            {
                this.shape = shape;
                this.index = index;
                this.point = point;
            }

            public void AssignSearchValues(VoronoiShape shape, int index, Vector2 point)
            {
                this.shape = shape;
                this.index = index;
                this.point = point;
            }

            public void Finalise(Vector2 cornerLocation)
            {
                shape.points.Add(cornerLocation);
                VMaths.GiftWrap(ref shape.points);
            }
        }

        public static void FixVoronoiDiagramCorners(List<VoronoiShape> vShapes, float boundaryWidth, float boundaryHeight)
        {
            float halfWidth = boundaryWidth / 2.0f;
            float halfHeight = boundaryHeight / 2.0f;

            SearchValues topLeftSearch = new SearchValues(vShapes[0], 0, vShapes[0].points[0]);
            SearchValues topRightSearch = new SearchValues(vShapes[0], 0, vShapes[0].points[0]);

            SearchValues bottomLeftSearch = new SearchValues(vShapes[0], 0, vShapes[0].points[0]);
            SearchValues bottomRightSearch = new SearchValues(vShapes[0], 0, vShapes[0].points[0]);

            for (int i = 0; i < vShapes.Count; i++)
            {
                VoronoiShape currentShape = vShapes[i];
                for(int pointIndex = 0; pointIndex < currentShape.points.Count; pointIndex++)
                {
                    Vector2 currentPoint = currentShape.points[pointIndex];

                    // Top searches
                    if (Mathf.Approximately(currentPoint.x, topLeftSearch.point.x))
                    {
                        if (currentShape.centre.y > topLeftSearch.shape.centre.y)
                        {
                            topLeftSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                        }
                    }
                    else if (currentPoint.x < topLeftSearch.point.x)
                    {
                        topLeftSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                    }

                    if (Mathf.Approximately(currentPoint.x, topRightSearch.point.x))
                    {
                        if (currentShape.centre.y > topRightSearch.shape.centre.y)
                        {
                            topRightSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                        }
                    }
                    else if (currentPoint.x > topRightSearch.point.x)
                    {
                        topRightSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                    }

                    // bottom searches
                    if (Mathf.Approximately(currentPoint.x, bottomLeftSearch.point.x))
                    {
                        if (currentShape.centre.y < bottomLeftSearch.shape.centre.y)
                        {
                            bottomLeftSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                        }
                    }
                    else if (currentPoint.x < bottomLeftSearch.point.x)
                    {
                        bottomLeftSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                    }

                    if (Mathf.Approximately(currentPoint.x, bottomRightSearch.point.x))
                    {
                        if (currentShape.centre.y < bottomRightSearch.shape.centre.y)
                        {
                            bottomRightSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                        }
                    }
                    else if (currentPoint.x > bottomRightSearch.point.x)
                    {
                        bottomRightSearch.AssignSearchValues(currentShape, pointIndex, currentPoint);
                    }
                }
            }

            Vector2 topLeft = new Vector2(-halfWidth, halfHeight);
            Vector2 topRight = new Vector2(halfWidth, halfHeight);

            Vector2 bottomLeft = new Vector2(-halfWidth, -halfHeight);
            Vector2 bottomRight = new Vector2(halfWidth, -halfHeight);

            topLeftSearch.Finalise(topLeft);
            topRightSearch.Finalise(topRight);

            bottomLeftSearch.Finalise(bottomLeft);
            bottomRightSearch.Finalise(bottomRight);
        }

        public void LloydRelaxation()
        {
            List<Vector2> centroidPoints = new List<Vector2>();
            foreach(VoronoiShape shape in vShapes)
            {
                centroidPoints.Add(shape.GetCentroid());
            }

            InitDeluanay(centroidPoints);
            InitVoronoi(m_debugBoundaryDist, m_boundaryWidth, m_boundaryHeight);
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
        public bool isBoundaryShape = false;

        public VoronoiShape(DelToVPoint delVPoint, float debugBoundaryDist, float boundaryWidth, float boundaryHeight)
        {
            debugIndex = delVPoint.delPoint.index;
            centre = delVPoint.delPoint.point;

            foreach (DelPoint neighbour in delVPoint.delPoint.connectedDelPoints)
            {
                neighbours.Add(neighbour.index);
            }

            CookieCutter.CookieBox cookieBox = new CookieCutter.CookieBox(boundaryWidth, boundaryHeight);

            foreach (VoronoiPoint vPoint in delVPoint.connectedVPoints)
            {
                points.Add(vPoint.position);
            }

            // Find the potential extended points to add
            var pointsToAdd = FindPotentialBoundaryPoints(delVPoint, debugBoundaryDist);

            // combine the points and found points into a big megaList
            List<Vector2> combined = new List<Vector2>(points);
            foreach(var point in pointsToAdd)
            {
                combined.Add(point);
            }

            // Giftwrapping will remove points that do not lie on the convex hull.
            // However we expect all these points to be on the hull.
            VMaths.GiftWrap(ref combined);

            // If any points are missing then this means the the shape identified potential boundary points when it shouldn't have
            if(combined.Count == points.Count + pointsToAdd.Count)
            {
                points = combined;
            }
            else
            {
                // Points were missing from the gift wrap therefore we will use the old point list as it is the correct form of the shape
                // still need to gift wrap it.
                GiftWrap();
            }

            CookieCutter.CookieCutterShape(ref points, boundaryWidth, boundaryHeight);
        }

        List<Vector2> FindPotentialBoundaryPoints(DelToVPoint delVPoint, float debugBoundaryDist)
        {
            List<Vector2> pointstoAdd = new List<Vector2>();
            if (delVPoint.connectedVPoints.Count == 1)
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

                if (emptyConnections.Count == 2)
                {
                    foreach (Connection emptyConnect in emptyConnections)
                    {
                        FindBoundaryPoint(pointstoAdd, emptyConnect, vPoint, debugBoundaryDist);
                    }
                }
                else if (emptyConnections.Count == 3)
                {
                    // This is the only vPoint in the diagram

                    // need to 2 find connections that point towards outside
                    foreach (Connection connect in emptyConnections)
                    {
                        float connectDot = GetDotCompareValue(connect, vPoint, delVPoint);
                        // if the dot product between 
                        if (connectDot < 0)
                        {
                            FindBoundaryPoint(pointstoAdd, connect, vPoint, debugBoundaryDist);
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
                        if (connect.isEmpty)
                        {
                            emptyConnections.Add(connect);
                        }
                    }

                    if (emptyConnections.Count > 1)
                    {
                        // need to find connection that points towards outside
                        Connection target = emptyConnections[0];
                        float lowestDot = GetDotCompareValue(target, vPoint, delVPoint);

                        for (int i = 1; i < emptyConnections.Count; i++)
                        {
                            Connection currentConnection = emptyConnections[i];
                            float currentDot = GetDotCompareValue(currentConnection, vPoint, delVPoint);
                            if (currentDot < lowestDot)
                            {
                                target = currentConnection;
                                lowestDot = currentDot;
                            }
                        }

                        FindBoundaryPoint(pointstoAdd, target, vPoint, debugBoundaryDist);
                    }
                    else
                    {
                        // safe to use the empty connection
                        FindBoundaryPoint(pointstoAdd, emptyConnections[0], vPoint, debugBoundaryDist);
                    }
                }
            }
            else
            {
                // Get vPoints with empty connections
                List<VoronoiPoint> emptyPoints = new List<VoronoiPoint>();

                foreach (VoronoiPoint vPoint in delVPoint.connectedVPoints)
                {
                    if (vPoint.hasEmptyConnection)
                    {
                        emptyPoints.Add(vPoint);
                    }
                }

                // Shapes on the outside can only ever have 2 V points with empty Connections
                if (emptyPoints.Count == 2)
                {
                    // Check if emptyPoints are not neighbours with each other
                    if (!emptyPoints[0].IsNeighbour(emptyPoints[1].index))
                    {
                        // They are not neighbour which means we can extend the shape outwards as it lies on the outside of the Voronoi pattern
                        foreach (var vPoint in emptyPoints)
                        {
                            List<Connection> emptyConnections = new List<Connection>();

                            foreach (Connection connect in vPoint.connectedPoints)
                            {
                                if (connect.isEmpty)
                                {
                                    emptyConnections.Add(connect);
                                }

                            }

                            if (emptyConnections.Count == 1)
                            {
                                FindBoundaryPoint(pointstoAdd, emptyConnections[0], vPoint, debugBoundaryDist);
                            }
                            else if (emptyConnections.Count == 2)
                            {
                                // Need to find the connection that belongs to this shape
                                // This should be the one that is pointing towards the centre closer
                                Vector2 vToDel = (delVPoint.delPoint.point - vPoint.position).normalized;

                                Vector2 firstBiSector = emptyConnections[0].GetBiSector(vPoint);
                                float firstDot = Vector2.Dot(vToDel, firstBiSector.normalized);

                                Vector2 secondBiSector = emptyConnections[1].GetBiSector(vPoint);
                                float secondDot = Vector2.Dot(vToDel, secondBiSector.normalized);

                                Connection towardsShape = emptyConnections[0];
                                if (firstDot < secondDot)
                                {
                                    towardsShape = emptyConnections[1];
                                }

                                FindBoundaryPoint(pointstoAdd, towardsShape, vPoint, debugBoundaryDist);
                            }
                        }
                    }
                }
            }

            return pointstoAdd;
        }

        void FindBoundaryPoint(List<Vector2> points, Connection connection, VoronoiPoint vPoint, float debugBoundaryDist)
        {
            if (!connection.isEmpty)
            {
                // Don't need to bother find a point as this connection leads to another voronoi point
                return;
            }

            isBoundaryShape = true;

            Vector2 vPosition = vPoint.position;
            Vector2 dir = connection.GetBiSector(vPoint);

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
            VMaths.GiftWrap(ref points);
        }

        public Vector2 GetCentroid()
        {
            Vector2 averagePos = Vector2.zero;
            foreach(Vector2 point in points)
            {
                averagePos += point;
            }

            averagePos /= points.Count;
            return averagePos;
        }
    }
}
