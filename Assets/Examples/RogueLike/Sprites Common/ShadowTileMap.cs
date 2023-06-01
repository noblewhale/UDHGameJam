namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    /// <summary>Automatically creates a shadowRuleTile on this Tilemap wherever mapToCopy has tiles.</summary>
    /// <remarks>
    /// This is used both in game and in the editor when painting tiles.
    /// </remarks>
    [ExecuteInEditMode]
    public class ShadowTileMap : MonoBehaviour
    {
        /// <summary>The Tilemap component on this game object to add shadow tiles to.</summary>
        Tilemap myMap;
        /// <summary>The Tilemap to shadow.</summary>
        public Tilemap mapToCopy;
        /// <summary>The shadow tile.</summary>
        public RuleTile shadowRuleTile;

        /// <summary>Adds a listener for changes to the mapToCopy and sets tileMap on DungeonObject</summary>
        void OnEnable()
        {
            myMap = GetComponent<Tilemap>();
            
            // Listen for tile change events so that we can update shadow tiles if the mapToCopy changes
            Tilemap.tilemapTileChanged += OnTileChange;

            // Assign shadow tilemap to DungeonObject so it can SetColor for lighting
            var pos = new Vector3Int(0, 0, 0);
            for (pos.x = myMap.cellBounds.min.x; pos.x < myMap.cellBounds.max.x; pos.x++)
            {
                for (pos.y = myMap.cellBounds.min.y; pos.y < myMap.cellBounds.max.y; pos.y++)
                {
                    var tileToCopy = mapToCopy.GetTile(pos);
                    if (tileToCopy && tileToCopy is DungeonRuleTile)
                    {
                        var dungeonObject = mapToCopy.GetInstantiatedObject(pos)?.GetComponent<DungeonObject>();
                        if (dungeonObject)
                        {
                            if (!dungeonObject.associatedTilemaps.Contains(myMap))
                            {
                                dungeonObject.associatedTilemaps.Add(myMap);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Removes the tile change listener</summary>
        void OnDisable()
        {
            Tilemap.tilemapTileChanged -= OnTileChange;
        }

        /// <summary>Update the shadow tile to match any tiles that changed in mapToCopy.</summary>
        /// <param name="map">The Tilemap that changed</param>
        /// <param name="syncTiles">The list of changes</param>
        void OnTileChange(Tilemap map, Tilemap.SyncTile[] syncTiles)
        {
            // Only interested in changes to mapToCopy, not other Tilemaps
            if (map != mapToCopy) return;

            foreach (var syncTile in syncTiles)
            {
                if (syncTile.tile is DungeonRuleTile)
                {
                    // Tile exists. Add or set shadow tile to match.
                    if (!myMap.HasTile(syncTile.position) && syncTile.tile != null)
                    {
                        Undo.RegisterCompleteObjectUndo(myMap, "Added shadow tile");
                        myMap.SetTile(syncTile.position, shadowRuleTile);
                        var dungeonObject = mapToCopy.GetInstantiatedObject(syncTile.position)?.GetComponent<DungeonObject>();
                        if (!dungeonObject.associatedTilemaps.Contains(myMap))
                        {
                            dungeonObject.associatedTilemaps.Add(myMap);
                        }
                    }
                }
                else if (syncTile.tile == null)
                {
                    // Tile was removed, remove shadow tile if it exists
                    myMap.SetTile(syncTile.position, null);
                }
            }
        }
    }
}
