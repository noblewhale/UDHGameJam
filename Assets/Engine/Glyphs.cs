using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glyphs : MonoBehaviour
{
    public Color damageFlashColor = Color.red;
    public SpriteRenderer[] glyphs;
	Color[] originalGlyphColors;
    #pragma warning disable 0414
    DungeonObject owner;
    #pragma warning restore 0414

    void Awake () 
	{
        glyphs = GetComponentsInChildren<SpriteRenderer>(true);
		originalGlyphColors = new Color[glyphs.Length];
		for (int i = 0; i < glyphs.Length; i++)
		{
			originalGlyphColors[i] = glyphs[i].color;
		}
        owner = GetComponentInParent<DungeonObject>();
	}

	public void SetRevealed(bool isRevealed)
    {
        gameObject.SetActive(isRevealed);
	}
    
    public void SetInView(bool isInView)
    {
        for (int i = 0; i < glyphs.Length; i++)
        {
            if (!isInView) glyphs[i].color = originalGlyphColors[i] / 2;
            else glyphs[i].color = originalGlyphColors[i];
        }
    }

    public void DamageFlash(float animationTime)
    {
        if (animationTime < .15f)
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i].color = Color.white;
            }
        }
        else if (animationTime < .3f)
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i].color = damageFlashColor;
            }
        }
        else
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i].color = originalGlyphColors[i];
            }
        }
    }
}
