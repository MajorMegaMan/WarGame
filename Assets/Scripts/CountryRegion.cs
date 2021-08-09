using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CountryRegion : MonoBehaviour
{
    SpriteShapeController m_spriteShapeController;

    private void Awake()
    {
        m_spriteShapeController = GetComponent<SpriteShapeController>();
    }

    public void InitialiseShape(Vector3[] points)
    {
        Spline spline = m_spriteShapeController.spline;
        spline.Clear();
        for (int i = 0; i < points.Length; i++)
        {
            spline.InsertPointAt(i, points[i]);
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
        m_spriteShapeController.BakeCollider();
    }
}
