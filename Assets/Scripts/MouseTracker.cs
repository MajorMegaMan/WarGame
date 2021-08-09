using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseTracker : MonoBehaviour
{
    public Color highlightColour = Color.white;

    [Header("Mouse Events")]
    public UnityEvent Enter;
    public UnityEvent Stay;
    public UnityEvent Exit;

    UnityEngine.U2D.SpriteShapeRenderer m_spriteRenderer;

    Color m_defaultColour;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<UnityEngine.U2D.SpriteShapeRenderer>();
        m_defaultColour = m_spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        Enter.Invoke();
        m_spriteRenderer.color = highlightColour;
    }

    private void OnMouseOver()
    {
        Stay.Invoke();
    }

    private void OnMouseExit()
    {
        Exit.Invoke();
        m_spriteRenderer.color = m_defaultColour;
    }
}
