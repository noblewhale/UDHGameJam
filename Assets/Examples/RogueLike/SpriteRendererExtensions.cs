namespace Noble.DungeonCrawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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
