namespace Noble.DungeonCrawler
{
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    [CreateAssetMenu]
    public class DungeonRuleTile : RuleTile
    {
        public bool isMask = false;

        public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject)
        {
            base.StartUp(position, tilemap, instantiatedGameObject);

            var tileMapComponent = tilemap.GetComponent<Tilemap>();

            if (instantiatedGameObject != null)
            {
                // Some vudu to allow us to see the Tile's GameObject in the hierarchy
                instantiatedGameObject.hideFlags &= ~HideFlags.HideInHierarchy;
#if UNITY_EDITOR
                try { EditorApplication.DirtyHierarchyWindowSorting(); } catch { }
#endif
                // Undo gameobject rotation caused by rules because that's how we want it to work
                // The TileSpriteMask script handles rotating the masks to match the tile rotation
                instantiatedGameObject.transform.localRotation = Quaternion.identity;
            }

            return true;
        }
    }
}