using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public int intialWidth = 5;
    public int intialHeight = 5;
    public float initalCellSize = 1.0f;
    
    GridMap<Tile.Type> grid;

    Tile[,] tiles;

    public GameObject tilePrefab;
    public Texture2D spriteMap;

    Tile.Type value = 0;

    void Awake()
    {
        grid = new GridMap<Tile.Type>(intialWidth, intialHeight, initalCellSize, Vector3.zero);

        tiles = new Tile[intialWidth, intialHeight];

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                GameObject newObject = Instantiate(tilePrefab, grid.GetWorldPosition(x, y), Quaternion.identity, this.transform);
                tiles[x, y] = newObject.GetComponent<Tile>();
                newObject.name = "Tile: " + x + ", " + y;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Debug.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x, y + 1));
                Debug.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x + 1, y));
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Utilities.GetMousePosition();
            grid.SetValue(mousePos, value++);
            grid.GetInfo(mousePos, out int targetX, out int targetY, out Tile.Type targetVal);
            Debug.Log("Clicked: " + targetX + "," + targetY + " = " + targetVal.ToString());

            // Set Tile
            Rect rec = Rect.zero;
            rec.x = 0;
            rec.y = 0;
            rec.width = 64;
            rec.height = 64;

            Sprite sprite = Sprite.Create(spriteMap, rec, Vector2.zero);
            SetTileSprite(mousePos, sprite);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Utilities.GetMousePosition();
            grid.SetValue(mousePos, value++);
            grid.GetInfo(mousePos, out int targetX, out int targetY, out Tile.Type targetVal);
            Debug.Log("Clicked: " + targetX + "," + targetY + " = " + targetVal.ToString());

            // Set Tile
            Rect rec = Rect.zero;
            rec.x = 64;
            rec.y = 0;
            rec.width = 64;
            rec.height = 64;

            Sprite sprite = Sprite.Create(spriteMap, rec, Vector2.zero, 64);
            SetTileSprite(mousePos, sprite);
        }
    }

    void SetTileSprite(Vector3 worldPosition, Sprite sprite)
    {
        grid.GetXY(worldPosition, out int x, out int y);
        SetTileSprite(x, y, sprite);
    }

    void SetTileSprite(int x, int y, Sprite sprite)
    {
        tiles[x, y].SetSprite(sprite);
    }
}
