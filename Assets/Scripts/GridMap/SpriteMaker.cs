using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileSprites", menuName = "ScriptableObjects/SpriteMaker", order = 1)]
public class SpriteMaker : ScriptableObject
{
    public Texture2D textureAtlas;

    public int horizontalSpriteCount = 1;
    public int pixelWidth = 128;
    public int pixelHeight = 128;

    public float pixelsPerUnit = 128;

    public Sprite[] CreateSprites()
    {
        Mathf.Clamp(horizontalSpriteCount, 1, int.MaxValue);
        Mathf.Clamp(pixelWidth, 1, int.MaxValue);
        Mathf.Clamp(pixelsPerUnit, 1, float.MaxValue);

        Sprite[] sprites = new Sprite[horizontalSpriteCount];

        for(int x = 0; x < horizontalSpriteCount; x++)
        {
            Rect rec = Rect.zero;

            rec.x = x * pixelWidth;
            rec.y = 0; // this will eventually contain height var
            rec.width = pixelWidth;
            rec.height = pixelHeight;

            sprites[x] = Sprite.Create(textureAtlas, rec, Vector2.zero, pixelsPerUnit);
        }

        return sprites;
    }
}
