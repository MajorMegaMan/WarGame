using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Voronoi.Helpers
{
    public class VMaths
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
        [System.Flags]
        public enum BoundsDir
        {
            inside = 0,
            up = 1,
            right = 2,
            down = 4,
            left = 8,

            upRight = up | right,
            upLeft = up | left,

            downRight = down | right,
            downLeft = down | left,

            horizontal = left | right,
            vertical = up | down
        }

        public class ShapeExtrudingPoints
        {
            public VoronoiShape shape;
            public List<int> pointIndices;
            public List<BoundsDir> pointDir;

            public int extrudingCount { get { return pointIndices.Count; } }

            public ShapeExtrudingPoints()
            {
                pointIndices = new List<int>();
                pointDir = new List<BoundsDir>();
            }
        }

        public static void CookieCutterShape(ref List<Vector2> points, float width, float height)
        {
            // Find all points outside bounds
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;

            // Search all points in shape to find points that may lie outside the bounds
            ShapeExtrudingPoints extrudingPoints = new ShapeExtrudingPoints();
            List<int> safePoints = new List<int>();

            for (int i = 0; i < points.Count; i++)
            {
                BoundsDir boundsDir = PointOutSideBounds(points[i], halfWidth, halfHeight);
                if (boundsDir != BoundsDir.inside)
                {
                    // point is outside shape
                    extrudingPoints.pointIndices.Add(i);
                    extrudingPoints.pointDir.Add(boundsDir);
                }
                else
                {
                    safePoints.Add(i);
                }
            }


            if (extrudingPoints.extrudingCount > 0)
            {
                // Has points outside
                if (extrudingPoints.extrudingCount == 1)
                {
                    // Shape is not on the outside of the voronoi pattern but is should be clamped
                    // Add in an extra position as two lines will cross the boundary

                    int index = extrudingPoints.pointIndices[0];
                    Vector2 firstResult = CalcOffsetToBoundary(points[index], points[GetBefore(index, points.Count)], extrudingPoints.pointDir[0], halfWidth, halfHeight);
                    Vector2 lastResult = CalcOffsetToBoundary(points[index], points[GetAfter(index, points.Count)], extrudingPoints.pointDir[0], halfWidth, halfHeight);

                    Vector2 orig = points[index];
                    points[index] += firstResult;
                    points.Add(orig + lastResult);
                    VMaths.GiftWrap(ref points);
                }
                else
                {
                    // Shape has many points out of bounds which means only the first and last need to be clamped, the rest can be deleted
                    // clamp first to not confuse the indices list, delete later
                    // use first point index - 1 and last point index + 1 to find directions
                    BoundaryBreakIndices breakOutIndices = FindBoundaryBreakIndices(extrudingPoints, points.Count);

                    int exitIndex = breakOutIndices.exit;
                    int enterIndex = breakOutIndices.enter;

                    int beforeExit = breakOutIndices.beforeExit;
                    int afterEnter = breakOutIndices.afterEnter;

                    Vector2 firstResult = CalcOffsetToBoundary(points[exitIndex], points[beforeExit], extrudingPoints.pointDir[0], halfWidth, halfHeight);

                    Vector2 lastResult = CalcOffsetToBoundary(points[enterIndex], points[afterEnter], extrudingPoints.pointDir[extrudingPoints.extrudingCount - 1], halfWidth, halfHeight);

                    points[exitIndex] += firstResult;
                    points[enterIndex] += lastResult;

                    // delete middle points
                    safePoints.Insert(0, exitIndex);
                    safePoints.Add(enterIndex);

                    RemoveBoundaryPoints(ref points, safePoints);
                }
            }

            // Use found points

            // Check if points should be 
        }

        public static Vector2 ClampPoint(Vector2 pointToClamp, BoundsDir boundsDirFlag, float halfWidth, float halfHeight)
        {
            Vector2 clampResult = pointToClamp;
            if ((BoundsDir.horizontal & boundsDirFlag) != 0)
            {
                clampResult.x = Mathf.Clamp(pointToClamp.x, -halfWidth, halfWidth);
            }
            else if((BoundsDir.vertical & boundsDirFlag) != 0)
            {
                clampResult.y = Mathf.Clamp(pointToClamp.y, -halfHeight, halfHeight);
            }

            return clampResult;
        }

        public static Vector2 CalcOffsetToBoundary(Vector2 start, Vector2 end, BoundsDir startBoundsFlag, float halfWidth, float halfHeight)
        {
            Vector2 direction = end - start;
            Vector2 clampedPoint = ClampPoint(start, startBoundsFlag, halfWidth, halfHeight);
            Vector2 pointToBoundary = clampedPoint - start;
            Vector2 result = VMaths.ProjectVector(pointToBoundary, direction);

            float test = Mathf.Abs(start.x + result.x);

            bool whatThefuck = test > halfWidth;
            if(whatThefuck)
            {
                Debug.Log(test + " > " + halfWidth);
            }
            else
            {
                Debug.Log(test + " is not Greater than " + halfWidth);
            }

            // need to re check if this was clamped properly
            if (test > halfWidth)
            {
                clampedPoint = ClampPoint(start, BoundsDir.horizontal, halfWidth, halfHeight);
                pointToBoundary = clampedPoint - start;
                result = VMaths.ProjectVector(pointToBoundary, direction);
            }
            else if(Mathf.Abs(start.y + result.y) > halfHeight)
            {
                clampedPoint = ClampPoint(start, BoundsDir.vertical, halfWidth, halfHeight);
                pointToBoundary = clampedPoint - start;
                result = VMaths.ProjectVector(pointToBoundary, direction);
            }

            return result;
        }

        public static BoundsDir PointOutSideBounds(Vector2 point, float halfWidth, float halfHeight)
        {
            BoundsDir boundsDir = 0;
            if (point.y > halfHeight)
            {
                boundsDir = boundsDir | BoundsDir.up;
            }
            else if (point.y < -halfHeight)
            {
                boundsDir = boundsDir | BoundsDir.down;
            }

            if (point.x > halfWidth)
            {
                boundsDir = boundsDir | BoundsDir.right;
            }
            else if (point.x < -halfWidth)
            {
                boundsDir = boundsDir | BoundsDir.left;
            }

            return boundsDir;
        }

        public struct BoundaryBreakIndices
        {
            public int exit;
            public int enter;
            public int beforeExit;
            public int afterEnter;
        }

        public static BoundaryBreakIndices FindBoundaryBreakIndices(ShapeExtrudingPoints extrudingPoints, int pointsCount)
        {
            BoundaryBreakIndices indices = new BoundaryBreakIndices();

            // The positions of the shapes move in a counter clockwise order. 
            // Therefore the lowest index will be the left most to left-down most and the highest index will be the most counter clockwise to the first point
            //
            //   highest o --o
            //          /     \
            // lowest  o       \
            //         \        o
            //          \      /
            //           o----o
            //
            // we want the lowest index and highest index that lie out side the boundary
            int enter = extrudingPoints.pointIndices[extrudingPoints.extrudingCount - 1];
            int exit = extrudingPoints.pointIndices[0];

            int afterEnter = GetAfter(enter, pointsCount);
            int beforeExit = GetBefore(exit, pointsCount);

            bool foundEnter = false;
            int prev = enter;

            for (int i = 0; i < extrudingPoints.extrudingCount; i++)
            {
                int current = extrudingPoints.pointIndices[i];
                int beforeCurrent = GetBefore(current, pointsCount);
                int afterCurrent = GetAfter(current, pointsCount);

                if (!foundEnter && beforeCurrent != prev)
                {
                    enter = prev;
                    afterEnter = GetAfter(prev, pointsCount);

                    exit = current;
                    beforeExit = beforeCurrent;

                    foundEnter = true;
                    break;
                }

                prev = current;
            }

            indices.exit = exit;
            indices.enter = enter;

            indices.beforeExit = beforeExit;
            indices.afterEnter = afterEnter;

            return indices;
        }

        public static void RemoveBoundaryPoints(ref List<Vector2> points, List<int> safePoints)
        {
            List<Vector2> resultPoints = new List<Vector2>();
            foreach (int index in safePoints)
            {
                resultPoints.Add(points[index]);
            }

            points = resultPoints;
            VMaths.GiftWrap(ref points);
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
