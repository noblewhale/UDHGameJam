namespace Noble.TileEngine
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Glyphs : MonoBehaviour
    {
        public Color damageFlashColor = Color.red;
        public List<Glyph> glyphs = new List<Glyph>();
        bool isLit;

        void Awake()
        {
            glyphs = new List<Glyph>(GetComponentsInChildren<Glyph>(true));

            //gameObject.SetActive(false);
        }

        public void SetRevealed(bool isRevealed)
        {
            gameObject.SetActive(isRevealed);
        }

        public void SetLit(bool isLit)
        {
            this.isLit = isLit;
        }

        public void DamageFlash(float animationTime)
        {
            if (animationTime < .25f)
            {
                for (int i = 0; i < glyphs.Count; i++)
                {
                    glyphs[i].extraTint = Color.white;
                }
            }
            else if (animationTime < .5f)
            {
                for (int i = 0; i < glyphs.Count; i++)
                {
                    glyphs[i].extraTint = damageFlashColor;
                }
            }
            else
            {
                for (int i = 0; i < glyphs.Count; i++)
                {
                    glyphs[i].extraTint = Color.white;
                }
            }
        }

        private void Update()
        {
            foreach (Glyph glyph in glyphs)
            {
                if (glyph == null || glyph.sprite == null) continue;
                glyph.sprite.color = glyph.originalColor - (Color.white - glyph.tint) - (Color.white - glyph.extraTint);
                if (!isLit)
                {
                    glyph.sprite.color = new Color(glyph.sprite.color.r / 2, glyph.sprite.color.g / 2, glyph.sprite.color.b / 2, glyph.sprite.color.a);
                }
            }
        }
    }
}