namespace Noble.TileEngine
{
    using System;
    using UnityEngine;

    [RequireComponent(typeof(SpriteRenderer))]
    public class Glyph : MonoBehaviour
    {
        public Color tint = Color.white;
        public Color extraTint = Color.white;
        public Color originalColor;

        SpriteRenderer _sprite;
        public SpriteRenderer sprite
        {
            get 
            { 
                if (_sprite == null)
                {
                    _sprite = GetComponent<SpriteRenderer>();
                    originalColor = _sprite.color;
                }
                return _sprite; 
            }
        }

        internal void ResetColor()
        {
            sprite.color = originalColor;
        }

        void Awake()
        {
            originalColor = sprite.color;
        }
    }
}