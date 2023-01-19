using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TestRuleTileScript : RuleTile 
{
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject)
    {
        bool good = base.StartUp(position, tilemap, instantiatedGameObject);

        if (instantiatedGameObject != null)
        {
            instantiatedGameObject.hideFlags &= ~HideFlags.HideInHierarchy;
#if UNITY_EDITOR
            try { EditorApplication.DirtyHierarchyWindowSorting(); } catch { }
#endif
        }


        return good;
    }
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (tileData.gameObject != null)
        {
            tileData.gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
#if UNITY_EDITOR
            try { EditorApplication.DirtyHierarchyWindowSorting(); } catch { }
#endif
        }

        base.GetTileData(position, tilemap, ref tileData);
    }
}