namespace Noble.DungeonCrawler
{
    using UnityEngine;

    [ExecuteInEditMode]
    public class SpriteColorBlendTwoColor : MonoBehaviour
    {
        MaterialPropertyBlock props;
        SpriteRenderer spriteRenderer;
        Sprite sprite;
        Rect spriteTextureRect;

        public Color color1, color2;
        public float gradientScale;

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
            props.SetFloat("_GradientScale", gradientScale);
            spriteRenderer.SetPropertyBlock(props);
        }
    }
}