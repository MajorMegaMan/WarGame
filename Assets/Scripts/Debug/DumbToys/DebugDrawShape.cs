using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Voronoi.Helpers;

public class DebugDrawShape : MonoBehaviour
{
    public Color lineColour = Color.red;

    [Header("Boundary")]
    public float mapWidth = 10.0f;
    public float mapHeight = 10.0f;
    public Color mapColour = Color.black;

    [Header("CookieCutter")]
    public Color cookieShapeColour = Color.green;
}
