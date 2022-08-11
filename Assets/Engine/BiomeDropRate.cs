namespace Noble.TileEngine
{
    using System;

    [Serializable]
    public class BiomeDropRate : DropRate
    {
        public int minQuantityPerBiome;
        public int maxQuantityPerBiome;
    }
}