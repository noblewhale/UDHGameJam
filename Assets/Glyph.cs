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
        unlitColor = sprite.color / 2;
        unlitColor.a = sprite.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLit)
        {
            sprite.color = originalColor - (Color.white - tint) - (Color.white - extraTint);
        }
        else
        {
            sprite.color = unlitColor - (Color.white - tint) - (Color.white - extraTint);
        }
    }
}
