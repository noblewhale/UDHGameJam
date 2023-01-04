namespace Noble.TileEngine
{
    public class MentalMap
    {
        int width => Map.instance.width;
        int height => Map.instance.height;

        bool[][] hasTileBeenRevealed;

        public MentalMap()
        {
            hasTileBeenRevealed = new bool[height][];
            for (int y = 0; y < height; y++) hasTileBeenRevealed[y] = new bool[width];
        }
    }
}
