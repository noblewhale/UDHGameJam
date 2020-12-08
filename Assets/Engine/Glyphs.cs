using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glyphs : MonoBehaviour
{
    public Color damageFlashColor = Color.red;
    public Glyph[] glyphs;

    void Awake () 
	{
        glyphs = GetComponentsInChildren<Glyph>(true);
	}

	public void SetRevealed(bool isRevealed)
    {
        gameObject.SetActive(isRevealed);
	}
    
    public void SetLit(bool isLit)
    {
        for (int i = 0; i < glyphs.Length; i++)
        {
            glyphs[i].isLit = isLit;
        }
    }

    public void DamageFlash(float animationTime)
    {
        if (animationTime < .25f)
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i].extraTint = Color.white;
            }
        }
        else if (animationTime < .5f)
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i].extraTint = damageFlashColor;
            }
        }
        else
        {
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i].extraTint = Color.white;
            }
        }
    }
}
