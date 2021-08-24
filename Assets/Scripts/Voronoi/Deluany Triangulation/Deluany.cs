using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi;
using Voronoi.Helpers;

namespace Voronoi.Deluany
{
    public class DelaunyMap
    {
        public List<DelPoint> delPoints;
        public List<DelTriangle> delTris;

        public DelaunyMap(List<Vector2> points)
        {
            delPoints = new List<DelPoint>();
            delTris = new List<DelTriangle>();

            for (int i = 0; i < points.Count; i++)
            {
                delPoints.Add(new DelPoint(points[i], i));
            }
            delTris = CalcTriangles(delPoints);
        }

        public DelaunyMap(List<Vector2> points, float maxRadius)
        {
            delPoints = new List<DelPoint>();
            delTris = new List<DelTriangle>();

            for(int i = 0; i < points.Count; i++)
            {
                delPoints.Add(new DelPoint(points[i], i));
            }
            delTris = CalcTriangles(delPoints, maxRadius);
        }

        public static List<DelTriangle> CalcTriangles(List<DelPoint> delPoints)
        {
            int triCount = 0;
            List<DelTriangle> delTriangles = new List<DelTriangle>();

            for (int a = 0; a < delPoints.Count - 2; a++)
            {
                for (int b = a + 1; b < delPoints.Count - 1; b++)
                {
                    for (int c = b + 1; c < delPoints.Count; c++)
                    {
                        DelTriangle potentialTri = new DelTriangle(delPoints[a], delPoints[b], delPoints[c]);

                        if (ConfirmTriangleAgainstPointList(potentialTri, delPoints, a, b, c))
                        {
                            potentialTri.SetIndex(triCount);
                            triCount++;
                            delTriangles.Add(potentialTri);
                        }
                    }
                }
            }

            ConnectDelTriangles(delTriangles);

            return delTriangles;
        }

        public static List<DelTriangle> CalcTriangles(List<DelPoint> delPoints, float maxRadius)
        {
            int triCount = 0;
            List<DelTriangle> delTriangles = new List<DelTriangle>();
        
            for(int a = 0; a < delPoints.Count - 2; a++)
            {
                for(int b = a + 1; b < delPoints.Count - 1; b++)
                {
                    for(int c = b + 1; c < delPoints.Count; c++)
                    {
                        DelTriangle potentialTri = new DelTriangle(delPoints[a], delPoints[b], delPoints[c]);

                        if(potentialTri.GetTriangle().CalcCircumcentreRadius() < maxRadius)
                        {
                            if (ConfirmTriangleAgainstPointList(potentialTri, delPoints, a, b, c))
                            {
                                potentialTri.SetIndex(triCount);
                                triCount++;
                                delTriangles.Add(potentialTri);
                            }
                        }
                    }
                }
            }

            ConnectDelTriangles(delTriangles);

            return delTriangles;
        }

        static bool ConfirmTriangleAgainstPointList(DelTriangle tri, List<DelPoint> pointList, int a, int b, int c)
        {
            bool isGoodTriangle = true;
            int d = 0;

            while(d < pointList.Count)
            {
                if(d == a)
                {
                    d += 1;
                }
                if(d == b)
                {
                    d += 1;
                }
                if(d == c)
                {
                    d += 1;
                }

                if(d >= pointList.Count)
                {
                    break;
                }

                Vector2 pointD = pointList[d].point;
                d++;

                if(!FindIsDeluanyTriangle(tri.GetTriangle(), pointD))
                {
                    isGoodTriangle = false;
                    break;
                }
            }

            return isGoodTriangle;
        }

        static bool FindIsDeluanyTriangle(Triangle tri, Vector2 pointD)
        {
            Vector3 aFancy = GetFancyNumberThing(tri.pointA, pointD);
            Vector3 bFancy = GetFancyNumberThing(tri.pointB, pointD);
            Vector3 cFancy = GetFancyNumberThing(tri.pointC, pointD);

            // Find the determinant
            float aPart = aFancy.x * (bFancy.y * cFancy.z - bFancy.z * cFancy.y);
            float bPart = aFancy.y * (bFancy.x * cFancy.z - bFancy.z * cFancy.x);
            float cPart = aFancy.z * (bFancy.x * cFancy.y - bFancy.y * cFancy.x);

            float determinant = aPart - bPart + cPart;

            bool isClockWise = tri.FindWindingOrder();

            if(isClockWise)
            {
                return determinant > 0;
            }
            else
            {
                return determinant < 0;
            }
        }

        static Vector3 GetFancyNumberThing(Vector2 triPoint, Vector2 pointD)
        {
            Vector3 result = Vector3.zero;
            result.x = triPoint.x - pointD.x;
            result.y = triPoint.y - pointD.y;
            result.z = result.x * result.x + result.y * result.y;

            return result;
        }

        public Vector2 CalcMeanCentre()
        {
            Vector2 meanCentre = Vector2.zero;
            foreach (DelPoint point in delPoints)
            {
                meanCentre += point.point;
            }
            meanCentre /= delPoints.Count;
            return meanCentre;
        }

        static void ConnectDelTriangles(List<DelTriangle> delTriangles)
        {
            // Check connections between all found triangles
            for (int i = 0; i < delTriangles.Count - 1; i++)
            {
                for (int j = i + 1; j < delTriangles.Count; j++)
                {
                    delTriangles[i].IdentifyConnections(delTriangles[j]);
                }
            }

            // Connect del Points together
            foreach (DelTriangle delTri in delTriangles)
            {
                delTri.ConnectDelPoints();
            }
        }
    }

    public class DelPoint
    {
        public int index { get; private set; }
        public Vector2 point;

        public List<DelPoint> connectedDelPoints;

        public DelPoint(Vector2 point, int index)
        {
            this.point = point;
            this.index = index;
            connectedDelPoints = new List<DelPoint>();
        }

        public bool IsInTriangle(DelTriangle delTri)
        {
            if(delTri.pointA.index == index || delTri.pointB.index == index || delTri.pointC.index == index)
            {
                return true;
            }
            return false;
        }

        public void ConnectToDelPoint(DelPoint target)
        {
            if(connectedDelPoints.Contains(target))
            {
                // If the target is already in the list don't add it
                return;
            }

            connectedDelPoints.Add(target);
        }
    }

    public class Connection
    {
        public int owner;
        public int otherTriangle;
        public DelPoint startPoint;
        public DelPoint endPoint;

        // if owner is the same as otherTriangle, then it will be treated as not having a connection
        public bool isEmpty { get { return owner == otherTriangle; } }

        public Connection(int owner, DelPoint start, DelPoint end)
        {
            this.owner = owner;
            otherTriangle = owner;
            startPoint = start;
            endPoint = end;
        }

        public Vector2 GetMidPoint()
        {
            Vector2 vector = endPoint.point - startPoint.point;
            vector *= 0.5f;
            return vector + startPoint.point;
        }

        public bool ProcessConnection(Connection connection)
        {
            bool containsStart = false;
            bool containsEnd = false;
            if (startPoint.index == connection.startPoint.index || startPoint.index == connection.endPoint.index)
            {
                // start is a match
                containsStart = true;
            }
            if(endPoint.index == connection.startPoint.index || endPoint.index == connection.endPoint.index)
            {
                // end is a match
                containsEnd = true;
            }

            if(containsStart && containsEnd)
            {
                otherTriangle = connection.owner;
                connection.otherTriangle = owner;
                return true;
            }
            return false;
        }

        public Vector2 GetBiSector(VoronoiPoint vPoint)
        {
            return GetBiSector(vPoint.position, vPoint.delTri.GetTriangle().FindMeanAverage());
        }

        public Vector2 GetBiSector(Vector2 vPosition, Vector2 meanCentre)
        {
            Vector2 midPoint = GetMidPoint();
            Vector2 dir = (midPoint - vPosition).normalized;

            Vector3 meanToMidDir = (midPoint - meanCentre).normalized;

            if (Vector2.Dot(dir, meanToMidDir) < 0)
            {
                dir *= -1;
            }

            return dir;
        }
    }

    public class DelTriangle
    {
        public int index;

        public DelPoint pointA;
        public DelPoint pointB;
        public DelPoint pointC;

        public List<Connection> connectedTris;

        public DelTriangle(DelPoint pointA, DelPoint pointB, DelPoint pointC)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.pointC = pointC;

            index = 0;
            connectedTris = null;
        }

        public void SetIndex(int index)
        {
            this.index = index;

            connectedTris = new List<Connection>();
            connectedTris.Add(new Connection(index, pointA, pointB));
            connectedTris.Add(new Connection(index, pointB, pointC));
            connectedTris.Add(new Connection(index, pointC, pointA));
        }

        public Triangle GetTriangle()
        {
            return new Triangle(pointA.point, pointB.point, pointC.point);
        }

        public Vector2 CalcCircumcentre()
        {
            return GetTriangle().CalcCircumcentre();
        }

        public bool IsSharingTwoPoints(DelTriangle otherTri)
        {
            int shareCount = 0;

            if (pointA.IsInTriangle(otherTri))
            {
                shareCount++;
            }
            if (pointB.IsInTriangle(otherTri))
            {
                shareCount++;
            }
            if (pointC.IsInTriangle(otherTri))
            {
                shareCount++;
            }
            return shareCount > 1;
        }

        // returns the connection of this triangle.
        // returns null if there are no connections between triangles
        public Connection IdentifyConnections(DelTriangle otherTri)
        {
            for(int i = 0; i < connectedTris.Count; i++)
            {
                // if connection matches any connection in this triangle
                for(int j = 0; j < otherTri.connectedTris.Count; j++)
                {
                    if(connectedTris[i].ProcessConnection(otherTri.connectedTris[j]))
                    {
                        return connectedTris[i];
                    }
                }
            }
            return null;
        }

        public void ConnectDelPoints()
        {
            pointA.ConnectToDelPoint(pointB);
            pointA.ConnectToDelPoint(pointC);

            pointB.ConnectToDelPoint(pointA);
            pointB.ConnectToDelPoint(pointC);

            pointC.ConnectToDelPoint(pointA);
            pointC.ConnectToDelPoint(pointB);
        }
    }

    [System.Serializable]
    public struct Triangle
    {
        public Vector2 pointA;
        public Vector2 pointB;
        public Vector2 pointC;

        public Triangle(Vector2 pointA, Vector2 pointB, Vector2 pointC)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.pointC = pointC;
        }

        public bool FindWindingOrder()
        {
            // Add extra dimension for cross product use
            Vector3 A = pointA;
            Vector3 B = pointB;
            Vector3 C = pointC;

            // find directions from the first point
            Vector3 dir1 = B - A;
            Vector3 dir2 = C - A;

            // use normal of triangle, currently only using 2D so this will always be forwqard in z
            Vector3 normal = Vector3.forward;

            // cross product of the two triangle directions
            Vector3 cross = Vector3.Cross(dir1, dir2);

            // if the dot product is negative it will be clockwise? << can't remember which winding order is the result
            return Vector3.Dot(cross, normal) < 0;
        }

        public Vector2 CalcCircumcentre()
        {
            DVector2 pointA = new DVector2(this.pointA);
            DVector2 pointB = new DVector2(this.pointB);
            DVector2 pointC = new DVector2(this.pointC);

            DVector2 aTob = pointB - pointA;
            DVector2 bToc = pointC - pointB;
            DVector2 cToa = pointA - pointC;

            DVector2 aToc = pointC - pointA;
            DVector2 bToa = pointA - pointB;
            DVector2 cTob = pointB - pointC;

            //float angleA = Vector2.Angle(aTob, aToc);
            //float angleB = Vector2.Angle(bToc, bToa);
            //float angleC = Vector2.Angle(cToa, cTob);

            double angleA = System.Math.Acos(DVector2.Dot(aTob, aToc) / (aTob.magnitude * aToc.magnitude));
            double angleB = System.Math.Acos(DVector2.Dot(bToc, bToa) / (bToc.magnitude * bToa.magnitude));
            double angleC = System.Math.Acos(DVector2.Dot(cToa, cTob) / (cToa.magnitude * cTob.magnitude));


            double sin2A = System.Math.Sin(2 * angleA);
            double sin2B = System.Math.Sin(2 * angleB);
            double sin2C = System.Math.Sin(2 * angleC);

            DVector2 dCircum = DVector2.zero;

            dCircum.x = (pointA.x * sin2A) + (pointB.x * sin2B) + (pointC.x * sin2C);
            dCircum.y = (pointA.y * sin2A) + (pointB.y * sin2B) + (pointC.y * sin2C);

            dCircum.x /= sin2A + sin2B + sin2C;
            dCircum.y /= sin2A + sin2B + sin2C;

            //circumcentre /= sin2A + sin2B + sin2C;
            Vector2 circumcentre = dCircum.GetVector2();

            return circumcentre;
        }

        public float CalcCircumcentreRadius()
        {
            Vector2 aTob = pointB - pointA;
            Vector2 bToc = pointC - pointB;
            Vector2 cToa = pointA - pointC;

            float A = aTob.magnitude;
            float B = bToc.magnitude;
            float C = cToa.magnitude;

            float radius = (A * B * C) / Mathf.Sqrt((A + B + C) * (B + C - A) * (C + A - B) * (A + B - C));
            return radius;
        }

        static float sign(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            return (point1.x - point3.x) * (point2.y - point3.y) - (point2.x - point3.x) * (point1.y - point3.y);
        }

        public bool ContainsPoint(Vector2 point)
        {
            float d1, d2, d3;
            bool hasNeg, hasPos;

            d1 = sign(point, pointA, pointB);
            d2 = sign(point, pointB, pointC);
            d3 = sign(point, pointC, pointA);

            hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        public Vector2 FindMeanAverage()
        {
            Vector2 triMean = Vector2.zero;
            triMean += pointA;
            triMean += pointB;
            triMean += pointC;
            triMean /= 3;
            return triMean;
        }
    }
}
