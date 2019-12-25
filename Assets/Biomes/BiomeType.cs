using System.Collections;
using UnityEngine;

[CreateAssetMenu]
public class BiomeType : ScriptableObject
{
    public BiomeDropRate[] objects;

    virtual public void PreProcessMap(Map map, RectInt area)
    {
    }
}
