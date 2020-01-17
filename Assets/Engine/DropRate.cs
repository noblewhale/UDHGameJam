using System;

[Serializable]
public class DropRate
{
    public DungeonObject item;
    public int minQuantity;
    public int maxQuantity;
    public float probability;

    public string[] onlySpawnOn;
    public string[] dontSpawnOn;
    public bool requireSpawnable = true;
}