using UnityEngine;

[CreateAssetMenu]
public class BiomeType : ScriptableObject
{
    public SpawnRate[] creatures;
    public BiomeDropRate[] items;
    public BiomeDropRate[] floors;
    public BiomeDropRate[] walls;
    public BiomeDropRate[] doors;

    public BiomeDropRate[] GetSpawnRatesForBaseType(Map.TileType baseType)
    {
        switch (baseType)
        {
            case Map.TileType.FLOOR: return floors;
            case Map.TileType.DOOR: return doors;
            case Map.TileType.WALL: return walls;
            default:
                Debug.LogError("Invalid base tile type: " + baseType.ToString());
                return null;
        }
    }
}
