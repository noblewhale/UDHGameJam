namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public static class SpriteRendererExtensions
    {
        public static Bounds GetCombinedBounds(this GameObject parent)
        {
            var childRenderers = parent.GetComponentsInChildren<SpriteRenderer>();
            Bounds combinedBounds = childRenderers[0].bounds;
            foreach (var renderer in childRenderers)
            {
                if (renderer.transform == parent.transform) continue;

                combinedBounds.Encapsulate(renderer.bounds);
            }

            return combinedBounds;
        }
    }
}
