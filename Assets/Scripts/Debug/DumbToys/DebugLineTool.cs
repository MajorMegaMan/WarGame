using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLineTool : MonoBehaviour
{
    public Vector2 start = Vector2.zero;
    public Vector2 end = Vector2.zero;

    public Color colour = Color.white;

    [Range(0.0f, 1.0f)]
    public float range = 1.0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = colour;
        Vector2 dir = end - start;

        Gizmos.DrawLine(start, start + (dir * range));
    }
}
