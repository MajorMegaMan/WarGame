using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voronoi.Deluany;

public class DebugDrawTriangle : MonoBehaviour
{
    public Triangle triangle = new Triangle(Vector2.right, Vector2.up, -Vector2.right);

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
        Gizmos.color = Color.red;

        Gizmos.DrawLine(triangle.pointA, triangle.pointB);
        Gizmos.DrawLine(triangle.pointB, triangle.pointC);
        Gizmos.DrawLine(triangle.pointC, triangle.pointA);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(triangle.CalcCircumcentre(), triangle.CalcCircumcentreRadius());
    }
}
