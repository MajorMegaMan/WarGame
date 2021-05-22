using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int initialWidth = 5;
    public int initialHeight = 5;
    public float initalCellSize = 1.0f;

    public CameraController camControl;

    [HideInInspector]
    public GridMap<Tile> grid;

    public GameObject tilePrefab;

    public SpriteMaker spriteMaker;
    Sprite[] tileSprites;

    void Awake()
    {
        grid = MapMaker.CreateGrid(tilePrefab, initialWidth, initialHeight, initalCellSize, Vector3.zero, this.transform);

        tileSprites = spriteMaker.CreateSprites();

        camControl.grid = grid;
    }

    private void Start()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                grid.GetValue(x, y).SetSprite(tileSprites[0]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Utilities.GetMousePosition();

            grid.GetInfo(mousePos, out int targetX, out int targetY, out Tile targetVal);
            if(targetVal != null)
            {
                Debug.Log("Clicked: " + targetVal.ToString());

                // Set Tile
                SetTileSprite(mousePos, tileSprites[0]);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Utilities.GetMousePosition();

            grid.GetInfo(mousePos, out int targetX, out int targetY, out Tile targetVal);
            if (targetVal != null)
            {
                Debug.Log("Clicked: " + targetVal.ToString());

                // Set Tile
                SetTileSprite(mousePos, tileSprites[1]);
            }
        }
    }

    void SetTileSprite(Vector3 worldPosition, Sprite sprite)
    {
        grid.GetXY(worldPosition, out int x, out int y);
        SetTileSprite(x, y, sprite);
    }

    void SetTileSprite(int x, int y, Sprite sprite)
    {
        grid.GetValue(x, y).SetSprite(sprite);
    }

    private void OnDrawGizmos()
    {
        GridMap<Tile.Type> grid = new GridMap<Tile.Type>(initialWidth, initialHeight, initalCellSize, Vector3.zero);

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Gizmos.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x, y + 1));
                Gizmos.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x + 1, y));
            }
        }

        Gizmos.DrawLine(grid.GetWorldPosition(0, grid.GetHeight()), grid.GetWorldPosition(grid.GetWidth(), grid.GetHeight()));
        Gizmos.DrawLine(grid.GetWorldPosition(grid.GetWidth(), grid.GetHeight()), grid.GetWorldPosition(grid.GetWidth(), 0));

        void DrawBox(Vector3 vert00, Vector3 vert01, Vector3 vert10, Vector3 vert11)
        {
            Gizmos.DrawLine(vert00, vert01);
            Gizmos.DrawLine(vert01, vert11);
            Gizmos.DrawLine(vert11, vert10);
            Gizmos.DrawLine(vert10, vert00);
        }

        Gizmos.color = Color.green;

        var bounds = grid.GetBounds();

        bounds.minX -= camControl.gridEdgeClamp;
        bounds.minY -= camControl.gridEdgeClamp;

        bounds.maxX += camControl.gridEdgeClamp;
        bounds.maxY += camControl.gridEdgeClamp;

        Vector3 vertex00 = new Vector3(bounds.minX, bounds.minY, 1);
        Vector3 vertex01 = new Vector3(bounds.minX, bounds.maxY, 1);
        Vector3 vertex10 = new Vector3(bounds.maxX, bounds.minY, 1);
        Vector3 vertex11 = new Vector3(bounds.maxX, bounds.maxY, 1);

        DrawBox(vertex00, vertex01, vertex10, vertex11);

        Gizmos.color = Color.red;

        Camera cam = Camera.main;

        Vector3 screen00 = new Vector3(0, 0, 1);
        Vector3 screen01 = new Vector3(0, cam.pixelHeight, 1);
        Vector3 screen10 = new Vector3(cam.pixelWidth, 0, 1);
        Vector3 screen11 = new Vector3(cam.pixelWidth, cam.pixelHeight, 1);

        void DrawPixelBox(Vector3 vert00, Vector3 vert01, Vector3 vert10, Vector3 vert11)
        {
            vert00 = cam.ScreenToWorldPoint(vert00);
            vert01 = cam.ScreenToWorldPoint(vert01);
            vert10 = cam.ScreenToWorldPoint(vert10);
            vert11 = cam.ScreenToWorldPoint(vert11);

            DrawBox(vert00, vert01, vert10, vert11);
        }

        DrawPixelBox(screen00, screen01, screen10, screen11);

        Gizmos.color = Color.yellow;

        // Find the size of the camera in pixels
        Vector3 camSize = Vector3.zero;
        camSize.x = cam.pixelWidth;
        camSize.y = cam.pixelHeight;

        // Find the world Position of the camSize
        camSize = cam.ScreenToWorldPoint(camSize) - cam.transform.position;

        Vector3 edgeWithCam00 = vertex00;
        Vector3 edgeWithCam01 = vertex01;
        Vector3 edgeWithCam10 = vertex10;
        Vector3 edgeWithCam11 = vertex11;

        edgeWithCam00.x += camSize.x;
        edgeWithCam01.x += camSize.x;
        edgeWithCam10.x -= camSize.x;
        edgeWithCam11.x -= camSize.x;

        edgeWithCam00.y += camSize.y;
        edgeWithCam01.y -= camSize.y;
        edgeWithCam10.y += camSize.y;
        edgeWithCam11.y -= camSize.y;

        //DrawBox(cam11, cam10, cam01, cam00);
        DrawBox(edgeWithCam00, edgeWithCam01, edgeWithCam10, edgeWithCam11);
    }
}
