using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Voronoi.Helpers
{
    public static class VMaths
    {
        public static void GiftWrap(ref List<Vector2> points)
        {
            // Check shape has enough points
            if (points.Count < 3)
            {
                Debug.LogError("This shape shouldn't exist. There is less than 3 vertices");
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

        public static Vector2 ProjectVector(Vector2 lhs, Vector2 rhs)
        {

            float angle = Vector2.Angle(lhs, rhs);
            // using angle cah
            float cosTheta = Mathf.Cos(angle * Mathf.Deg2Rad); // c
            float lhsDist = lhs.magnitude; // a

            // c = a / h
            // a = c * h
            // h = a / c

            float h = lhsDist / cosTheta;

            Vector2 result = rhs.normalized * h;

            // a . b = |a| * |b| * cos theta
            return result;
        }

        //Get the intersection between a line and a plane. 
        //If the line and plane are not parallel, the function outputs true, otherwise false.
        public static bool LinePlaneIntersection(out Vector2 intersection, Vector2 linePoint, Vector2 lineDir, Vector2 planeNormal, Vector2 planePoint)
        {

            float length;
            float dotNumerator;
            float dotDenominator;
            Vector2 vector;
            intersection = Vector2.zero;

            //calculate the distance between the linePoint and the line-plane intersection point
            dotNumerator = Vector2.Dot((planePoint - linePoint), planeNormal);
            dotDenominator = Vector2.Dot(lineDir, planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;

                //create a vector from the linePoint to the intersection point
                vector = lineDir * length;

                //get the coordinates of the line-plane intersection point
                intersection = linePoint + vector;

                return true;
            }

            //output not valid
            else
            {
                return false;
            }
        }

        public static bool LinePlaneIntersection(out Vector2 intersection, Vector2 linePoint, Vector2 lineDir, Plane plane)
        {
            return LinePlaneIntersection(out intersection, linePoint, lineDir, plane.normal, plane.normal * -plane.distance);
        }

        //Get the shortest distance between a point and a plane. The output is signed so it holds information
        //as to which side of the plane normal the point is.
        public static float SignedDistancePlanePoint(Vector2 planeNormal, Vector2 planePoint, Vector2 point)
        {
            return Vector2.Dot(planeNormal, (point - planePoint));
        }

        public static int IsOverlappingPlane(Plane plane, Vector2 lineStart, Vector2 lineEnd)
        {
            float startToPlane = plane.GetDistanceToPoint(lineStart);
            float endToPlane = plane.GetDistanceToPoint(lineEnd);

            bool startBehind = startToPlane < 0;
            bool endBehind = endToPlane < 0;

            if (startBehind != endBehind)
            {
                // line is overlapping plane
                return 0;
            }

            if (startBehind == false)
            {
                // line is fully in normal direction
                return 1;
            }
            else
            {
                // line is fully behind normal direction
                return -1;
            }
        }

        public static bool LineLineIntersection(out Vector2 intersect, Vector2 lineStart1, Vector2 lineVec1, Vector2 lineStart2, Vector2 lineVec2)
        {
            // Convert to vec3 for cross product use
            Vector3 lineDir1 = lineVec1;
            Vector3 lineDir2 = lineVec2;

            Vector3 vecDir3 = lineStart2 - lineStart1;
            Vector3 crossVec1and2 = Vector3.Cross(lineDir1, lineDir2);
            Vector3 crossVec3and2 = Vector3.Cross(vecDir3, lineDir2);

            float planarFactor = Vector3.Dot(vecDir3, crossVec1and2);

            //is coplanar, and not parrallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

                intersect = lineStart1 + (lineVec1 * s);
                return true;
            }
            else
            {
                intersect = Vector3.zero;
                return false;
            }
        }

        // Finds intersection point as well as if the lines segments are overlapping.
        // returns 0 = Two lines will never intersect. (parallel or colinear) : intersect defaults to zero.
        // returns 1 = Two lines will eventually intersect but the segments do not overlap. : intersect is found.
        // returns 2 = Two line segments have an intersection and are overlapping.
        public static int LineLineIntersectionConstraint(out Vector2 intersect, Vector2 lineStart1, Vector2 lineVec1, Vector2 lineStart2, Vector2 lineVec2)
        {
            // Convert to vec3 for cross product use
            Vector3 lineDir1 = lineVec1;
            Vector3 lineDir2 = lineVec2;

            Vector3 vecDir3 = lineStart2 - lineStart1;
            Vector3 crossVec1and2 = Vector3.Cross(lineDir1, lineDir2);
            Vector3 crossVec3and2 = Vector3.Cross(vecDir3, lineDir2);

            float planarFactor = Vector3.Dot(vecDir3, crossVec1and2);

            //is coplanar, and not parrallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

                intersect = lineStart1 + (lineVec1 * s);

                Vector2 line2toIntersect = intersect - lineStart2;
                float line2Dot = Vector2.Dot(line2toIntersect, lineVec2);


                if (s < 0 || s * s > lineVec1.sqrMagnitude || line2Dot < 0 || line2toIntersect.sqrMagnitude > lineVec2.sqrMagnitude)
                {
                    return 1;
                }

                return 2;
            }
            else
            {
                intersect = Vector3.zero;
                return 0;
            }
        }
    }

    struct DVector2
    {
        public double x;
        public double y;

        public double magnitude { get { return System.Math.Sqrt(x * x + y * y); } }
        public DVector2 normalized { get { return this / magnitude; } }

        static DVector2 _zero;
        public static DVector2 zero { get { return _zero; } }

        public DVector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public DVector2(Vector2 point)
        {
            this.x = point.x;
            this.y = point.y;
        }

        static DVector2()
        {
            _zero = new DVector2(0.0, 0.0);
        }

        public static DVector2 operator +(DVector2 lhs, DVector2 rhs)
        {
            lhs.x += rhs.x;
            lhs.y += rhs.y;
            return lhs;
        }

        public static DVector2 operator -(DVector2 lhs, DVector2 rhs)
        {
            lhs.x -= rhs.x;
            lhs.y -= rhs.y;
            return lhs;
        }

        public static DVector2 operator *(DVector2 vector, double scalar)
        {
            vector.x *= scalar;
            vector.y *= scalar;
            return vector;
        }

        public static DVector2 operator *(double scalar, DVector2 vector)
        {
            vector = vector * scalar;
            return vector;
        }

        public static DVector2 operator /(DVector2 vector, double scalar)
        {
            vector.x /= scalar;
            vector.y /= scalar;
            return vector;
        }

        public static double Dot(DVector2 lhs, DVector2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public Vector2 GetVector2()
        {
            Vector2 result = Vector2.zero;
            result.x = (float)x;
            result.y = (float)y;
            return result;
        }
    }

    public static class CookieCutter
    {
        public struct LineOverlap
        {
            Vector2 m_start;
            Vector2 m_end;

            int[] overlaps;
            Vector2[] intersections;

            int m_overlapStatus;

            public Vector2 start { get { return m_start; } }
            public Vector2 end { get { return m_end; } }
            public int topOverlap { get { return overlaps[0]; } private set { overlaps[0] = value; } }
            public int rightOverlap { get { return overlaps[1]; } private set { overlaps[1] = value; } }
            public int bottomOverlap { get { return overlaps[2]; } private set { overlaps[2] = value; } }
            public int leftOverlap { get { return overlaps[3]; } private set { overlaps[3] = value; } }

            // returns 1 = fully inside
            // returns 0 = overlapping
            // returns -1 = fully outside
            public int overlapStatus { get { return m_overlapStatus; } }
            public bool lineIsInside { get { return m_overlapStatus == 1; } }
            public bool lineIsOutside { get { return m_overlapStatus == -1; } }
            public bool lineIsOverlapping { get { return m_overlapStatus == 0; } }

            public LineOverlap(CookieBox cookieBox, Vector2 lineStart, Vector2 lineEnd)
            {
                m_start = lineStart;
                m_end = lineEnd;

                overlaps = new int[4];
                intersections = new Vector2[4];

                m_overlapStatus = 0;

                FindIntersections(cookieBox);
                m_overlapStatus = FindOverlapStatus(cookieBox);
            }

            void FindIntersections(Plane plane, int index)
            {
                overlaps[index] = VMaths.IsOverlappingPlane(plane, m_start, m_end);
                VMaths.LinePlaneIntersection(out intersections[index], m_start, (m_end - m_start).normalized, plane);
            }

            void FindIntersections(CookieBox cookieBox)
            {
                FindIntersections(cookieBox.topPlane, 0);
                FindIntersections(cookieBox.rightPlane, 1);
                FindIntersections(cookieBox.bottomPlane, 2);
                FindIntersections(cookieBox.leftPlane, 3);
            }

            public bool AllInside()
            {
                return topOverlap + rightOverlap + bottomOverlap + leftOverlap == 4;
            }

            public List<Vector2> GetIntersections()
            {
                List<Vector2> result = null;
                if (topOverlap == 0)
                {
                    if(result == null)
                    {
                        result = new List<Vector2>();
                    }
                    result.Add(intersections[0]);
                }
                if (rightOverlap == 0)
                {
                    if (result == null)
                    {
                        result = new List<Vector2>();
                    }
                    result.Add(intersections[1]);
                }
                if (bottomOverlap == 0)
                {
                    if (result == null)
                    {
                        result = new List<Vector2>();
                    }
                    result.Add(intersections[2]);
                }
                if (leftOverlap == 0)
                {
                    if (result == null)
                    {
                        result = new List<Vector2>();
                    }
                    result.Add(intersections[3]);
                }
                return result;
            }

            // returns 1 = fully inside
            // returns 0 = overlapping
            // returns -1 = fully outside
            int FindOverlapStatus(CookieBox cookieBox)
            {
                if (AllInside())
                {
                    return 1;
                }
                bool startPointInside = cookieBox.ContainsPoint(start);
                bool endPointInside = cookieBox.ContainsPoint(end);

                List<Vector2> intersections = cookieBox.GetIntersections(this);

                // If not the same, then they are overlapping
                if (startPointInside != endPointInside || intersections != null)
                {
                    return 0;
                }
                else
                {
                    // fully outside
                    return -1;
                }
            }
        }

        public struct CookieBox
        {
            public Plane[] planes;

            public float epsilon;

            public Plane topPlane { get { return planes[0]; } private set { planes[0] = value; } }
            public Plane rightPlane { get { return planes[1]; } private set { planes[1] = value; } }
            public Plane bottomPlane { get { return planes[2]; } private set { planes[2] = value; } }
            public Plane leftPlane { get { return planes[3]; } private set { planes[3] = value; } }

            public CookieBox(float width, float height, float epsilon = 0.00001f)
            {
                float halfWidth = width / 2.0f;
                float halfHeight = height / 2.0f;

                planes = new Plane[4];

                this.epsilon = epsilon;

                topPlane = new Plane(-Vector3.up, Vector3.up * halfHeight);
                rightPlane = new Plane(-Vector3.right, Vector3.right * halfWidth);
                bottomPlane = new Plane(Vector3.up, Vector3.up * -halfHeight);
                leftPlane = new Plane(Vector3.right, Vector3.right * -halfWidth);
            }

            public bool ContainsPoint(Vector2 point)
            {
                return ContainsPoint(point, epsilon);
            }

            public bool ContainsPoint(Vector2 point, float allowance)
            {
                foreach(Plane plane in planes)
                {
                    if (plane.GetDistanceToPoint(point) < 0 - allowance)
                    {
                        return false;
                    }
                }
                return true;
            }

            public LineOverlap FindLineOverlap(Vector2 lineStart, Vector2 lineEnd)
            {
                return new LineOverlap(this, lineStart, lineEnd);
            }

            // returns the index of the start point of the line that is overlapping or outside
            // returns -1 if no line was overlapping or outside
            public int FindNextLineOutside(List<Vector2> points, int startIndex)
            {
                // for pair of points, find if not inside all planes
                int currentIndex = startIndex;
                do
                {
                    int nextIndex = GetAfter(currentIndex, points.Count);

                    LineOverlap lineStatus = FindLineOverlap(points[currentIndex], points[currentIndex + 1]);
                    if (!lineStatus.lineIsInside)
                    {
                        // line could be outside or overlapping
                        return currentIndex;
                    }

                    currentIndex = nextIndex;
                } while (currentIndex != startIndex);

                {
                    LineOverlap lineStatus = FindLineOverlap(points[points.Count - 1], points[startIndex]);
                    if (!lineStatus.lineIsInside)
                    {
                        // line could be outside or overlapping
                        return points.Count - 1;
                    }
                }

                // If reached here, no line was found overlapping or outside the box
                return -1;
            }

            // returns the index of the start point of the line that is overlapping or outside
            // returns -1 if no line was overlapping or outside
            public int FindNextLineInside(List<Vector2> points, int startIndex)
            {
                // for pair of points, find if not inside all planes
                int currentIndex = startIndex;
                do
                {
                    int nextIndex = GetAfter(currentIndex, points.Count);

                    LineOverlap lineStatus = FindLineOverlap(points[currentIndex], points[nextIndex]);
                    if (lineStatus.lineIsInside)
                    {
                        // line is Indside
                        return currentIndex;
                    }

                    currentIndex = nextIndex;
                } while (currentIndex != startIndex);

                {
                    LineOverlap lineStatus = FindLineOverlap(points[points.Count - 1], points[startIndex]);
                    if (lineStatus.lineIsInside)
                    {
                        // line is Indside
                        return points.Count - 1;
                    }
                }

                // If reached here, no line was found overlapping or outside the box
                return -1;
            }

            public List<Vector2> GetIntersections(LineOverlap lineOverlap)
            {
                List<Vector2> lineIntersects = lineOverlap.GetIntersections();
                if(lineIntersects == null)
                {
                    return null;
                }

                List<Vector2> cookieIntersects = null;
                foreach(Vector2 point in lineIntersects)
                {
                    if(ContainsPoint(point))
                    {
                        if(cookieIntersects == null)
                        {
                            cookieIntersects = new List<Vector2>();
                        }
                        cookieIntersects.Add(point);
                    }
                }

                return cookieIntersects;
            }
        }

        struct ShapeLineOverlap
        {
            public LineOverlap lineOverLap;
            public int startIndex;
            public int endIndex;

            public ShapeLineOverlap(LineOverlap lineOverLap, int startIndex, int endIndex)
            {
                this.lineOverLap = lineOverLap;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
            }
        }

        public static void CookieCutterShape(ref List<Vector2> points, float width, float height)
        {
            // Set up planes
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;

            // Create box of planes
            CookieBox cookieBox = new CookieBox(width, height);

            // Find line overlaps with cookieBox
            // Do not include Lines that are completly outside the box
            List<ShapeLineOverlap> insideBox = new List<ShapeLineOverlap>();
            List<ShapeLineOverlap> overlapBox = new List<ShapeLineOverlap>();
            List<ShapeLineOverlap> outsideBox = new List<ShapeLineOverlap>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                int start = i;
                int end = i + 1;
                LineOverlap lineOverlap = cookieBox.FindLineOverlap(points[start], points[end]);
                switch(lineOverlap.overlapStatus)
                {
                    case 1:
                        {
                            insideBox.Add(new ShapeLineOverlap(lineOverlap, start, end));
                            break;
                        }
                    case 0:
                        {
                            overlapBox.Add(new ShapeLineOverlap(lineOverlap, start, end));
                            break;
                        }
                    case -1:
                        {
                            outsideBox.Add(new ShapeLineOverlap(lineOverlap, start, end));
                            break;
                        }
                }
            }

            int cycleStart = points.Count - 1;
            int cycleEnd = 0;
            LineOverlap cycleLineOverlap = cookieBox.FindLineOverlap(points[cycleStart], points[cycleEnd]);
            switch (cycleLineOverlap.overlapStatus)
            {
                case 1:
                    {
                        insideBox.Add(new ShapeLineOverlap(cycleLineOverlap, cycleStart, cycleEnd));
                        break;
                    }
                case 0:
                    {
                        overlapBox.Add(new ShapeLineOverlap(cycleLineOverlap, cycleStart, cycleEnd));
                        break;
                    }
                case -1:
                    {
                        outsideBox.Add(new ShapeLineOverlap(cycleLineOverlap, cycleStart, cycleEnd));
                        break;
                    }
            }

            List<Vector2> resultPoints = new List<Vector2>();

            // Safely add inside lines
            bool[] indexIsAdded = new bool[points.Count];

            void AddPointToResults(List<Vector2> targetShapePoints, int index)
            {
                if (!indexIsAdded[index])
                {
                    indexIsAdded[index] = true;
                    resultPoints.Add(targetShapePoints[index]);
                }
            }

            foreach(var shapeLine in insideBox)
            {
                AddPointToResults(points, shapeLine.startIndex);
                AddPointToResults(points, shapeLine.endIndex);
            }

            // Add intersections from the overlap points
            foreach (var shapeLine in overlapBox)
            {
                if(cookieBox.ContainsPoint(points[shapeLine.startIndex]))
                {
                    AddPointToResults(points, shapeLine.startIndex);
                }
                if (cookieBox.ContainsPoint(points[shapeLine.endIndex]))
                {
                    AddPointToResults(points, shapeLine.endIndex);
                }

                List<Vector2> intersections = cookieBox.GetIntersections(shapeLine.lineOverLap);
                foreach(Vector2 intersectPoint in intersections)
                {
                    Vector2 clamped = intersectPoint;
                    clamped.x = Mathf.Clamp(intersectPoint.x, -halfWidth, halfWidth);
                    clamped.y = Mathf.Clamp(intersectPoint.y, -halfHeight, halfHeight);

                    resultPoints.Add(clamped);
                }
            }

            // finalise pointList
            VMaths.GiftWrap(ref resultPoints);
            points = resultPoints;
        }

        public static int GetBefore(int index, int max)
        {
            int before = index - 1;
            if (index == 0)
            {
                before = max - 1;
            }
            return before;
        }

        public static int GetAfter(int index, int max)
        {
            int after = index + 1;
            after = after % max;
            return after;
        }
    }
}
