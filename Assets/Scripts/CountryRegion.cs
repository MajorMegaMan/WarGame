using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Voronoi;

public class CountryRegion : MonoBehaviour
{
    SpriteShapeController m_spriteShapeController;
    SpriteShapeRenderer m_spriteRenderer;
    List<CountryRegion> m_neighbours = new List<CountryRegion>();

    public Color highlightColour = Color.white;
    Color m_defaultColour;

    private void Awake()
    {
        m_spriteShapeController = GetComponent<SpriteShapeController>();
        m_spriteRenderer = GetComponent<SpriteShapeRenderer>();
        m_defaultColour = m_spriteRenderer.color;
    }

    public void InitialiseShape(VoronoiShape vShape, List<CountryRegion> countryRegions)
    {
        InitialiseShape(vShape.points);
        for(int i = 0; i < vShape.neighbours.Count; i++)
        {
            m_neighbours.Add(countryRegions[vShape.neighbours[i]]);
        }
    }

    void InitialiseShape(List<Vector2> points)
    {
        Spline spline = m_spriteShapeController.spline;
        spline.Clear();
        for (int i = 0; i < points.Count; i++)
        {
            spline.InsertPointAt(i, points[i]);
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
        m_spriteShapeController.BakeCollider();
    }

    public void ShowHighlight()
    {
        m_spriteRenderer.color = highlightColour;
        foreach(CountryRegion neighbour in m_neighbours)
        {
            neighbour.m_spriteRenderer.color = Color.blue;
        }
    }

    public void HideHighlight()
    {
        m_spriteRenderer.color = m_defaultColour;
        foreach (CountryRegion neighbour in m_neighbours)
        {
            neighbour.m_spriteRenderer.color = neighbour.m_defaultColour;
        }
    }
}
