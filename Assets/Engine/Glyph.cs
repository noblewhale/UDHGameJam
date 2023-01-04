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

        [NonSerialized]
        public SpriteRenderer sprite;

        void Start()
        {
            sprite = GetComponent<SpriteRenderer>();
            originalColor = sprite.color;
        }
    }
}