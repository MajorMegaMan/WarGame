using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public int initialWidth = 5;
    public int initialHeight = 5;
    public float initalCellSize = 1.0f;
    [HideInInspector]
    public GridMap<Tile> grid;

    public GameObject tilePrefab;

    public SpriteMaker spriteMaker;
    Sprite[] tileSprites;

    void Awake()
    {
        grid = new GridMap<Tile>(initialWidth, initialHeight, initalCellSize, Vector3.zero);

        tileSprites = spriteMaker.CreateSprites();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GameObject newObject = Instantiate(tilePrefab, grid.GetWorldPosition(x, y), Quaternion.identity, this.transform);

                grid.SetValue(x, y, newObject.GetComponent<Tile>());

                newObject.name = "Tile: " + x + ", " + y;
            }
        }
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
            Debug.Log("Clicked: " + targetVal.ToString());

            // Set Tile
            SetTileSprite(mousePos, tileSprites[0]);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Utilities.GetMousePosition();

            grid.GetInfo(mousePos, out int targetX, out int targetY, out Tile targetVal);
            Debug.Log("Clicked: " + targetVal.ToString());

            // Set Tile
            SetTileSprite(mousePos, tileSprites[1]);
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
    }
}
