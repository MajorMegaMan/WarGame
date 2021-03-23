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

    public Type type = Type.walkable;

    void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
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
