using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Glyph : MonoBehaviour
{
    public Color tint = Color.white;
    public Color extraTint = Color.white;
    public bool isLit;
    Color originalColor;
    Color unlitColor;

    [NonSerialized]
    public SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        originalColor = sprite.color;
    }

    // Update is called once per frame
    void Update()
    {
        sprite.color = originalColor - (Color.white - tint) - (Color.white - extraTint);
        if (!isLit)
        {
            sprite.color = new Color(sprite.color.r / 2, sprite.color.g / 2, sprite.color.b / 2, sprite.color.a);
        }
    }
}
