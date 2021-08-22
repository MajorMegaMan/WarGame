using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseTracker : MonoBehaviour
{
    [Header("Mouse Events")]
    public UnityEvent Enter;
    public UnityEvent Stay;
    public UnityEvent Exit;

    private void Awake()
    {

    }

    private void OnMouseEnter()
    {
        Enter.Invoke();
    }

    private void OnMouseOver()
    {
        Stay.Invoke();
    }

    private void OnMouseExit()
    {
        Exit.Invoke();
    }
}
