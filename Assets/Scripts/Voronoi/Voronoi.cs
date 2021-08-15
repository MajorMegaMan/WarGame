using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi.Deluany;

namespace Voronoi
{
    public static class VoronoiDiagram
    {
        public static List<VoronoiPoint> Create(List<Vector2> points)
        {
            List<DelPoint> delPoints = new List<DelPoint>();

            for (int i = 0; i < points.Count; i++)
            {
                delPoints.Add(new DelPoint(points[i], i));
            }
            return Create(delPoints);
        }

        public static List<VoronoiPoint> Create(List<DelPoint> delPoints)
        {
            List<DelTriangle> delTris = DelaunyMap.CalcTriangles(delPoints);
            return Create(delTris);
        }

        public static List<VoronoiPoint> Create(List<DelTriangle> delTris)
        {
            List<VoronoiPoint> points = new List<VoronoiPoint>();
            foreach (var tri in delTris)
            {
                VoronoiPoint vPoint = new VoronoiPoint();
                vPoint.index = tri.index;
                vPoint.position = tri.CalcCircumcentre();
                vPoint.connectedPoints = new List<int>();
                points.Add(vPoint);
            }

            for (int i = 0; i < points.Count; i++)
            {
                foreach (int index in delTris[i].connectedTris)
                {
                    points[i].connectedPoints.Add(index);
                }
            }

            return points;
        }

        public static List<DelToVPoint> FindShapes(List<VoronoiPoint> vPoints, DelaunyMap delMap)
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
    }

    public struct VoronoiPoint
    {
        public int index;
        public Vector2 position;
        public List<int> connectedPoints;
    }

    public struct DelToVPoint
    {
        public DelPoint delPoint;
        public List<VoronoiPoint> connectedVPoints;
    }
}
