using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glyphs : MonoBehaviour
{
    public Color damageFlashColor = Color.red;
    SpriteRenderer[] glyphs;
	Color[] originalGlyphColors;
    Coroutine damageFlashProcess;
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
        owner = GetComponent<DungeonObject>();
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

    public void TakeDamage(int v)
    {
        if (glyphs.Length > 0)
        {
            if (damageFlashProcess != null) StopCoroutine(damageFlashProcess);
            damageFlashProcess = StartCoroutine(DoDamageFlash());
        }
    }

    IEnumerator DoDamageFlash()
    {
        for (int i = 0; i < glyphs.Length; i++)
        {
            glyphs[i].color = damageFlashColor;
        }

        yield return new WaitForSeconds(.2f);

        for (int i = 0; i < glyphs.Length; i++)
        {
            glyphs[i].color = originalGlyphColors[i];
        }
        damageFlashProcess = null;
    }
}
