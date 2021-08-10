using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi.Deluany;

public class DebugDeluany : MonoBehaviour
{
    public List<Transform> points;

    bool initialised = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        List<DelPoint> delPoints = new List<DelPoint>();

        for(int i = 0; i < points.Count; i++)
        {
            delPoints.Add(new DelPoint(points[i].position, i));
        }

        var delTris = DelaunyMap.CalcTriangles(delPoints);

        void DrawTri(Triangle tri)
        {
            Gizmos.DrawLine(tri.pointA, tri.pointB);
            Gizmos.DrawLine(tri.pointB, tri.pointC);
            Gizmos.DrawLine(tri.pointC, tri.pointA);
        }

        foreach(var delTri in delTris)
        {
            DrawTri(delTri.GetTriangle());
        }

        Triangle firstTri = delTris[0].GetTriangle();

        Vector3 circumCentre = firstTri.CalcCircumcentre();
        float radius = firstTri.CalcCircumcentreRadius();
        Gizmos.DrawWireSphere(circumCentre, radius);
    }
}
