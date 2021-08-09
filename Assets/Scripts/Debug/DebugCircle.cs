using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCircle : MonoBehaviour
{
    public Color colour = Color.red;
    public float radius = 0.5f;

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
        Gizmos.color = colour;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
