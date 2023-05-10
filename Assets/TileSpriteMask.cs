namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    /// <summary>Use the sprite from the Tile occupied by this object as the mask for a SpriteMask</summary>
    /// <remarks>
    /// This is used both in game and in the editor when painting tiles.
    /// </remarks>
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteMask))]
    public class TileSpriteMask : MonoBehaviour
    {
        /// <summary>The parent DungeonObject, cached on first fetch.</summary>
        DungeonObject _dungeonObject;
        DungeonObject dungeonObject
        {
            get
            {
                if (_dungeonObject == null) _dungeonObject = GetComponentInParent<DungeonObject>(true);
                return _dungeonObject;
            }
        }
        
        /// <summary>The parent Tilemap, cached on first fetch.</summary>
        Tilemap _tileMap;
        Tilemap tileMap
        {
            get
            {
                if (_tileMap == null) _tileMap = GetComponentInParent<Tilemap>(true);
                return _tileMap;
            }
        }

        /// <summary>The parent SpiteMask, cached on first fetch.</summary>
        SpriteMask _mask;
        SpriteMask mask
        {
            get
            {
                if (_mask == null) _mask = GetComponent<SpriteMask>();
                return _mask;
            }
        }

        /// <summary>Make sure SpriteMask sprite matches the Tile sprite, and set the Tile color to transparent.</summary>
        void Update()
        {
            if (dungeonObject && tileMap && mask)
            {
                var position = tileMap.WorldToCell(dungeonObject.transform.position);
                transform.rotation = tileMap.GetTransformMatrix(position).rotation;
                mask.sprite = tileMap.GetSprite(position);
                tileMap.SetColor(position, Color.clear);
            }
        }
    }
}