namespace Noble.DungeonCrawler
{
    using UnityEngine;

    [ExecuteInEditMode]
    public class SpriteColorBlend : MonoBehaviour
    {
        MaterialPropertyBlock props;
        SpriteRenderer spriteRenderer;
        Sprite sprite;
        Rect spriteTextureRect;

        public float offset1, offset2;
        public Color color1, color2, color3;

        void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            sprite = spriteRenderer.sprite;

            props = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(props);
        }

        void Update()
        {
            spriteTextureRect = sprite.textureRect;
            Vector4 minMaxX = new Vector4(spriteTextureRect.xMin / sprite.texture.width, spriteTextureRect.xMax / sprite.texture.width, 0, 0);
            props.SetVector("_MinMaxX", minMaxX);
            props.SetColor("_FirstColor", color1);
            props.SetColor("_SecondColor", color2);
            props.SetColor("_ThirdColor", color3);
            props.SetVector("_GradientOffsets", new Vector4(offset1, offset2, 0, 0));
            spriteRenderer.SetPropertyBlock(props);
        }
    }
}