namespace Noble.TileEngine
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.TextCore;

    public class Glyphs : MonoBehaviour
    {
        public Color damageFlashColor = Color.red;
        List<Glyph> _glyphs;
        DungeonObject dungeonObject;

        public Color unityTileTint = Color.white;

        public List<Glyph> glyphs
        {
            get
            {
                if (_glyphs == null || _glyphs.Count == 0)
                {
                    _glyphs = new List<Glyph>(GetComponentsInChildren<Glyph>(true));
                }

                return _glyphs;
            }
        }
        bool isLit = true;

        private void Start()
        {
            dungeonObject = GetComponentInParent<DungeonObject>();
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

        public void ResetGlyphColors()
        {
            foreach (Glyph glyph in glyphs)
            {
                glyph.ResetColor();
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

                if (glyph == glyphs[0] && dungeonObject.tileMap)
                {
                    Color unityTileColor = unityTileTint;

                    if (!isLit)
                    {
                        unityTileColor = new Color(unityTileColor.r / 2, unityTileColor.g / 2, unityTileColor.b / 2, unityTileColor.a);
                    }
                    dungeonObject.tileMap.SetColor(dungeonObject.tileMap.WorldToCell(dungeonObject.transform.position), unityTileColor);
                }
            }
        }
    }
}