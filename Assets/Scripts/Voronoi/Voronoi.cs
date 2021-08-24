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

        public VoronoiDiagram(List<Vector2> points, float debugBoundaryDist, float boundaryWidth, float boundaryHeight)
        {
            delMap = new DelaunyMap(points);

            vPoints = CreateVPoints(delMap);
            vShapes = FindShapes(vPoints, delMap, debugBoundaryDist, boundaryWidth, boundaryHeight);
        }

        public VoronoiDiagram(List<Vector2> points, float debugBoundaryDist, float boundaryWidth, float boundaryHeight, float maxRadius = float.PositiveInfinity)
        {
            delMap = new DelaunyMap(points, maxRadius);

            vPoints = CreateVPoints(delMap);
            vShapes = FindShapes(vPoints, delMap, debugBoundaryDist, boundaryWidth, boundaryHeight);
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

            foreach (VoronoiPoint vPoint in delVPoint.connectedVPoints)
            {
                points.Add(vPoint.position);
            }

            FindPotentialBoundaryPoints(delVPoint, debugBoundaryDist);

            GiftWrap();

            CookieCutter.CookieCutterShape(ref points, boundaryWidth, boundaryHeight);
        }

        void FindPotentialBoundaryPoints(DelToVPoint delVPoint, float debugBoundaryDist)
        {
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
                        FindBoundaryPoint(emptyConnect, vPoint, debugBoundaryDist);
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
                                FindBoundaryPoint(emptyConnections[0], vPoint, debugBoundaryDist);
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

                                FindBoundaryPoint(towardsShape, vPoint, debugBoundaryDist);
                            }
                        }
                    }
                }
            }
        }

        void FindBoundaryPoint(Connection connection, VoronoiPoint vPoint, float debugBoundaryDist)
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
    }
}
