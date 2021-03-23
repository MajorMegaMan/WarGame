using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public enum Type
    {
        walkable,
        blocked
    }

    Type type = Type.walkable;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSprite(Sprite targetSprite)
    {
        spriteRenderer.sprite = targetSprite;
    }
}
