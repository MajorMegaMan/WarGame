using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            for(int i = 0; i < points.Count; i++)
            {
                delPoints.Add(new DelPoint(points[i], i));
            }
            delTris = CalcTriangles(delPoints);
        }

        public static List<DelTriangle> CalcTriangles(List<DelPoint> delPoints)
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

                        if(ConfirmTriangleAgainstPointList(potentialTri, delPoints, a, b, c))
                        {
                            potentialTri.index = triCount;
                            triCount++;
                            delTriangles.Add(potentialTri);
                        }
                    }
                }
            }

            // Check connections between all found triangles
            for(int i = 0; i < delTriangles.Count - 1; i++)
            {
                for(int j = i + 1; j < delTriangles.Count; j++)
                {
                    if(delTriangles[i].IsSharingTwoPoints(delTriangles[j]))
                    {
                        delTriangles[i].connectedTris.Add(delTriangles[j].index);
                        delTriangles[j].connectedTris.Add(delTriangles[i].index);
                    }
                }
            }

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
    }

    public struct DelPoint
    {
        public int index { get; private set; }
        public Vector2 point;

        public DelPoint(Vector2 point, int index)
        {
            this.point = point;
            this.index = index;
        }

        public bool IsInTriangle(DelTriangle delTri)
        {
            if(delTri.pointA.index == index || delTri.pointB.index == index || delTri.pointC.index == index)
            {
                return true;
            }
            return false;
        }
    }

    public struct DelTriangle
    {
        public int index;

        public DelPoint pointA;
        public DelPoint pointB;
        public DelPoint pointC;

        public List<int> connectedTris;

        public DelTriangle(DelPoint pointA, DelPoint pointB, DelPoint pointC)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.pointC = pointC;

            index = 0;
            connectedTris = new List<int>();
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
            if(pointA.IsInTriangle(otherTri))
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
    }

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
            Vector2 aTob = pointB - pointA;
            Vector2 bToc = pointC - pointB;
            Vector2 cToa = pointA - pointC;

            Vector2 aToc = pointC - pointA;
            Vector2 bToa = pointA - pointB;
            Vector2 cTob = pointB - pointC;

            //float angleA = Vector2.Angle(aTob, aToc);
            //float angleB = Vector2.Angle(bToc, bToa);
            //float angleC = Vector2.Angle(cToa, cTob);

            float angleA = Mathf.Acos(Vector3.Dot(aTob, aToc) / (aTob.magnitude * aToc.magnitude));
            float angleB = Mathf.Acos(Vector3.Dot(bToc, bToa) / (bToc.magnitude * bToa.magnitude));
            float angleC = Mathf.Acos(Vector3.Dot(cToa, cTob) / (cToa.magnitude * cTob.magnitude));


            float sin2A = Mathf.Sin(2 * angleA);
            float sin2B = Mathf.Sin(2 * angleB);
            float sin2C = Mathf.Sin(2 * angleC);

            Vector2 circumcentre = Vector2.zero;
            circumcentre.x = (pointA.x * sin2A) + (pointB.x * sin2B) + (pointC.x * sin2C);
            circumcentre.y = (pointA.y * sin2A) + (pointB.y * sin2B) + (pointC.y * sin2C);

            circumcentre /= sin2A + sin2B + sin2C;

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
    }
}
