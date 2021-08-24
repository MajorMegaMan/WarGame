using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawCircle : MonoBehaviour
{
    public Vector2 position = Vector2.zero;
    public float radius = 1.0f;
    public Color colour = Color.white;

    private void OnDrawGizmos()
    {
        Gizmos.color = colour;
        Gizmos.DrawWireSphere(position, radius);
    }
}
