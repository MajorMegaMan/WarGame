using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DrawGrid : MonoBehaviour
{
    public Testing gridManager;
    public Color colour = Color.black;

    GridMap<Tile> grid;

    bool running = false;

    private void Start()
    {
        grid = gridManager.grid;
        running = true;
    }

    private void OnDrawGizmos()
    {
        if(!running)
        {
            return;
        }

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Gizmos.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x, y + 1));
                Gizmos.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x + 1, y));
                Vector3 offset = Vector3.one * (gridManager.initalCellSize * 0.5f);
                offset.z = 0;
                Handles.color = colour;
                Handles.Label(grid.GetWorldPosition(x, y) + offset, grid.GetValue(x, y).type.ToString());
            }
        }

        Gizmos.DrawLine(grid.GetWorldPosition(0, grid.GetHeight()), grid.GetWorldPosition(grid.GetWidth(), grid.GetHeight()));
        Gizmos.DrawLine(grid.GetWorldPosition(grid.GetWidth(), grid.GetHeight()), grid.GetWorldPosition(grid.GetWidth(), 0));
    }
}
