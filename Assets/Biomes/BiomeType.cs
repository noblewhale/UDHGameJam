using UnityEngine;

[CreateAssetMenu]
public class BiomeType : ScriptableObject
{
    public SpawnRate[] creatures;
    public BiomeDropRate[] items;
}
