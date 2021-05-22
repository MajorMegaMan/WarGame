using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapMaker
{
    public static GridMap<Tile> CreateGrid(GameObject tilePrefab, int width, int height)
    {
        return CreateGrid(tilePrefab, width, height, 1.0f, Vector3.zero, null);
    }

    public static GridMap<Tile> CreateGrid(GameObject tilePrefab, int width, int height, Vector3 origin)
    {
        return CreateGrid(tilePrefab, width, height, 1.0f, origin, null);
    }

    public static GridMap<Tile> CreateGrid(GameObject tilePrefab, int width, int height, Transform parent)
    {
        return CreateGrid(tilePrefab, width, height, 1.0f, Vector3.zero, parent);
    }

    public static GridMap<Tile> CreateGrid(GameObject tilePrefab, int width, int height, float cellSize)
    {
        return CreateGrid(tilePrefab, width, height, cellSize, Vector3.zero, null);
    }

    public static GridMap<Tile> CreateGrid(GameObject tilePrefab, int width, int height, float cellSize, Vector3 origin)
    {
        return CreateGrid(tilePrefab, width, height, cellSize, origin, null);
    }

    public static GridMap<Tile> CreateGrid(GameObject tilePrefab, int width, int height, float cellSize, Vector3 origin, Transform parent)
    {
        GridMap<Tile> grid = new GridMap<Tile>(width, height, cellSize, origin);

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GameObject newObject = GameObject.Instantiate(tilePrefab, grid.GetWorldPosition(x, y), Quaternion.identity, parent);

                grid.SetValue(x, y, newObject.GetComponent<Tile>());

                newObject.name = "Tile: " + x + ", " + y;
            }
        }

        return grid;
    }
}
