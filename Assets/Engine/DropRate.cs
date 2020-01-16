using System;

[Serializable]
public class DropRate
{
    public DungeonObject item;
    public int minQuantity;
    public int maxQuantity;
    public float probability;

    public bool replaceObjects = false;
    public DungeonObject[] onlySpawnOn;
    public DungeonObject[] dontSpawnOn;
    public bool requireSpawnable = true;
}