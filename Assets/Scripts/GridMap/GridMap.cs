using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap<T>
{
    int width;
    int height;
    float cellSize;
    Vector3 origin;

    T[,] gridArray;

    AABB bounds;

    public struct AABB
    {
        public float minX;
        public float minY;
        public float maxX;
        public float maxY;
    }

    public GridMap(int width, int height, float cellSize, Vector3 origin)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        gridArray = new T[width, height];

        Vector3 minPos = GetWorldPosition(0, 0);
        Vector3 maxPos = GetWorldPosition(width - 1, height - 1);

        bounds = new AABB();
        bounds.minX = minPos.x;
        bounds.minY = minPos.y;

        bounds.maxX = maxPos.x + cellSize;
        bounds.maxY = maxPos.y + cellSize;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 result = Vector3.zero;
        result.x = x * cellSize;
        result.y = y * cellSize;
        result += origin;
        return result;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition.x - origin.x) / cellSize);
        y = Mathf.FloorToInt((worldPosition.y - origin.y) / cellSize);
    }

    public void SetValue(int x, int y, T value)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
        }
    }

    public T GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        return default(T);
    }

    public void SetValue(Vector3 worldPosition, T value)
    {
        GetXY(worldPosition, out int x, out int y);
        SetValue(x, y, value);
    }

    public T GetValue(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        return GetValue(x, y);
    }

    public void GetInfo(Vector3 worldPosition, out int x, out int y, out T value)
    {
        GetXY(worldPosition, out x, out y);
        value = GetValue(x, y);
    }

    public AABB GetBounds()
    {
        return bounds;
    }
}
