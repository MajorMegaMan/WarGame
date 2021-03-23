using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    ////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////
    // Mouse Position
    public static Vector3 GetMousePosition()
    {
        Vector3 vec = GetMousePositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0;
        return vec;
    }

    public static Vector3 GetMousePositionWithZ()
    {
        return GetMousePositionWithZ(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetMousePositionWithZ(Camera worldCamera)
    {
        return GetMousePositionWithZ(Input.mousePosition, worldCamera);
    }

    public static Vector3 GetMousePositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}
