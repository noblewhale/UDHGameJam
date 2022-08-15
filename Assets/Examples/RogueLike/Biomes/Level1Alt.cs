namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu]
    public class Level1Alt : Biome
    {
        // Stop partitioning at this size
        public int minBSPArea = 4;

        // Delay to use when animating the level gernation process
        float animationDelay = 0;//.15f;

        public BiomeDropRate[] nothings;
        public BiomeDropRate[] floors;
        public BiomeDropRate[] walls;
        public BiomeDropRate[] doors;
        public BiomeDropRate[] torches;
        public Biome scorpions;
        public Biome rats;
        public Biome items;
        public Biome electricTraps;
        public DungeonObject finalDoorPrefab;

        class Node
        {
            public Node left;
            public Node right;
            public Node parent;
            public RectIntExclusive area;
            public RectIntExclusive room;
        }

        Node root;

        struct TilePair
        {
            public Vector2Int tileA, tileB;

            public TilePair(Vector2Int tileA, Vector2Int tileB)
            {
                this.tileA = tileA;
                this.tileB = tileB;
            }
        }

        public enum TileType
        {
            NOTHING, FLOOR, WALL, DOOR, TORCH, ELECTRIC
        }

        public TileType[][] tiles;

        override public IEnumerator PreProcessMap(Map map)
        {
            tiles = new TileType[map.height][];
            for (int y = 0; y < map.height; y++) tiles[y] = new TileType[map.width];

            var itemsBiome = Instantiate(items);
            itemsBiome.area = area;
            subBiomes.Add(itemsBiome);

            yield return new WaitForSeconds(animationDelay * 2);
            root = new Node();
            root.area = area;
            yield return new WaitForSeconds(animationDelay);
            yield return Map.instance.StartCoroutine(GenerateAreas(root, 1));
            yield return Map.instance.StartCoroutine(GenerateRooms(root));
            var mapArea = new RectIntExclusive(0, 0, Map.instance.width, Map.instance.height);
            AddWalls();

            if (animationDelay != 0)
            {
                UpdateTiles(mapArea);
                yield return new WaitForSeconds(animationDelay);
            }
            PruneDoors();
            UpdateTiles(mapArea);

            //SpawnFinalDoor(map, area);

            base.PreProcessMap(map);
        }

        override public void DrawDebug(RectIntExclusive area)
        {
            base.DrawDebug(area);
            DrawArea(root);
        }

        void DrawArea(Node parent)
        {
            var map = Map.instance;
            if (parent == null) return;
            EditorUtil.DrawRect(map, parent.area, Color.blue);
            if (parent.left == null && parent.right == null && !parent.room.IsZero())
            {

                EditorUtil.DrawRect(map, parent.room, Color.red, new Vector2(.35f, .35f));
            }
            DrawArea(parent.left);
            DrawArea(parent.right);
        }

        void PruneDoors()
        {
            var map = Map.instance;
            for (int y = 0; y < map.height; y++)
            {
                for (int x = 0; x < map.width; x++)
                {
                    if (tiles[y][x] == TileType.DOOR)
                    {
                        if (HasFloorOnThreeSides(x, y)) tiles[y][x] = TileType.FLOOR;
                    }
                }
            }
        }

        bool HasFloorOnThreeSides(int x, int y)
        {
            var map = Map.instance;
            if (y <= 0 || y >= map.height - 1) return false;
            int floorCount = 0;
            int wrappedXLeft = map.WrapX(x - 1);
            int wrappedXRight = map.WrapX(x + 1);
            if (tiles[y][wrappedXLeft] == TileType.FLOOR) floorCount++;
            if (tiles[y][wrappedXRight] == TileType.FLOOR) floorCount++;
            if (tiles[y - 1][x] == TileType.FLOOR) floorCount++;
            if (tiles[y + 1][x] == TileType.FLOOR) floorCount++;

            if (floorCount >= 3) return true;
            return false;
        }

        bool IsDoorSpot(int x, int y, bool inverted = false)
        {
            var map = Map.instance;

            if (inverted)
            {
                int t = x;
                x = y;
                y = t;
            }

            if (y <= 0 || y >= map.height - 1) return false;

            int wrappedX = map.WrapX(x);
            int wrappedXLeft = map.WrapX(x - 1);
            int wrappedXRight = map.WrapX(x + 1);
            if (tiles[y][wrappedXLeft] == TileType.FLOOR && tiles[y][wrappedXRight] == TileType.FLOOR)
            {
                if (tiles[y - 1][wrappedX] == TileType.NOTHING && tiles[y + 1][wrappedX] == TileType.NOTHING)
                {
                    return true;
                }
            }
            if (tiles[y - 1][wrappedX] == TileType.FLOOR && tiles[y + 1][wrappedX] == TileType.FLOOR)
            {
                if (tiles[y][wrappedXLeft] == TileType.NOTHING && tiles[y][wrappedXRight] == TileType.NOTHING)
                {
                    return true;
                }
            }
            return false;
        }

        void AddWalls()
        {
            var map = Map.instance;
            for (int y = 0; y < map.height; y++)
            {
                for (int x = 0; x < map.width; x++)
                {
                    if (tiles[y][x] == TileType.NOTHING)
                    {
                        if (HasAdjacentFloor(x, y)) tiles[y][x] = TileType.WALL;
                    }
                }
            }
        }
        bool HasAdjacentFloor(int x, int y)
        {
            var map = Map.instance;
            int wrappedX = map.WrapX(x - 1);
            if (tiles[y][wrappedX] == TileType.FLOOR) return true;
            wrappedX = map.WrapX(x + 1);
            if (tiles[y][wrappedX] == TileType.FLOOR) return true;
            if (y > 0)
            {
                wrappedX = map.WrapX(x - 1);
                if (tiles[y - 1][wrappedX] == TileType.FLOOR) return true;
                wrappedX = map.WrapX(x + 1);
                if (tiles[y - 1][wrappedX] == TileType.FLOOR) return true;
                if (tiles[y - 1][x] == TileType.FLOOR) return true;
            }
            if (y < map.height - 1)
            {
                wrappedX = map.WrapX(x - 1);
                if (tiles[y + 1][wrappedX] == TileType.FLOOR) return true;
                wrappedX = map.WrapX(x + 1);
                if (tiles[y + 1][wrappedX] == TileType.FLOOR) return true;
                if (tiles[y + 1][x] == TileType.FLOOR) return true;
            }

            return false;
        }

        IEnumerator GenerateRooms(Node parent)
        {
            if (parent == null) yield break;

            if (parent.left == null && parent.right == null)
            {
                // Leaf node, actually generate a room
                int areaWidth = parent.area.width - 2;
                int areaHeight = parent.area.height - 2;
                int minWidth = Mathf.Max(1, areaWidth / 3);
                int minHeight = Mathf.Max(1, areaHeight / 3);
                int w = UnityEngine.Random.Range(minWidth, areaWidth);
                int h = UnityEngine.Random.Range(minHeight, areaHeight);
                int xMin = UnityEngine.Random.Range(parent.area.xMin + 1, parent.area.xMax - (w - 1));
                int yMin = UnityEngine.Random.Range(parent.area.yMin + 1, parent.area.yMax - (h - 1));
                var rect = new RectIntExclusive(xMin, yMin, w, h);
                parent.room = rect;

                for (int y = rect.yMin; y <= rect.yMax; y++)
                {
                    for (int x = rect.xMin; x <= rect.xMax; x++)
                    {
                        tiles[y][x] = TileType.FLOOR;
                    }
                }

                var roomCreatures = Instantiate(scorpions);
                roomCreatures.area = rect;
                subBiomes.Add(roomCreatures);

                var roomTraps = Instantiate(electricTraps);
                roomTraps.area = new RectIntExclusive(rect.xMin + 1, rect.yMin + 1, rect.width - 2, rect.height - 2);
                subBiomes.Add(roomTraps);

                for (int y = rect.yMin - 1; y <= rect.yMax + 1; y++)
                {
                    for (int x = rect.xMin - 1; x <= rect.xMax + 1; x++)
                    {
                        Map.instance.tileObjects[y][Map.instance.WrapX(x)].isAlwaysLit = true;
                        Map.instance.tileObjects[y][Map.instance.WrapX(x)].SetLit(true);
                    }
                }
                if (animationDelay != 0)
                {
                    UpdateTiles(parent.room);
                    yield return new WaitForSeconds(animationDelay);
                }
            }
            else
            {
                // Not a leaf node, keep traversing the tree
                yield return Map.instance.StartCoroutine(GenerateRooms(parent.left));
                yield return Map.instance.StartCoroutine(GenerateRooms(parent.right));

                // Ensure at least one path between the two subtrees
                if (parent.left.area.xMax == parent.right.area.xMax)
                {
                    // Vertical split
                    // Get a list of floor tiles at maximum y for each x in bottom split, and the min and max x values that have floor tiles
                    List<Vector2Int> topTilesInBottomSplit = GetTopFloorTiles(parent.left.area);

                    // Get a list of floor tiles at minimum y for each x in top split, and the min and max x values that have floor tiles
                    List<Vector2Int> bottomTilesInTopSplit = GetBottomFloorTiles(parent.right.area);

                    // Get the overlap between the two above lists
                    bool isConnected = false;
                    var overlappingTiles = GetOverlap(topTilesInBottomSplit, bottomTilesInTopSplit, true, out isConnected);
                    RectIntExclusive pathArea;

                    if (!isConnected)
                    {
                        if (overlappingTiles.Count != 0)
                        {
                            // Pick a random x position within the overlapping area to add a connecting path
                            int randomIndex = UnityEngine.Random.Range(0, overlappingTiles.Count);
                            int randomX = overlappingTiles[randomIndex].tileA.x;
                            pathArea = AddStraightConnectingPath(randomX, parent.left.area, parent.right.area, false);
                            if (animationDelay != 0)
                            {
                                UpdateTiles(pathArea);
                            }
                        }
                        else
                        {
                            var bottomTileIndex = UnityEngine.Random.Range(0, topTilesInBottomSplit.Count);
                            var topTileIndex = UnityEngine.Random.Range(0, bottomTilesInTopSplit.Count);
                            var bottomTile = topTilesInBottomSplit[bottomTileIndex];
                            var topTile = bottomTilesInTopSplit[topTileIndex];
                            AddAngledPath(bottomTile, topTile, true);
                            if (animationDelay != 0)
                            {
                                UpdateTiles(parent.area);
                            }
                        }
                        yield return new WaitForSeconds(animationDelay);
                    }
                }
                else
                {
                    // Horizontal split
                    // Get a list of floor tiles at maximum x for each y in left split, and the min and max y values that have floor tiles
                    List<Vector2Int> rightTilesInLeftSplit = GetRightFloorTiles(parent.left.area);

                    // Get a list of floor tiles at minimum x for each y in top split, and the min and max y values that have floor tiles
                    List<Vector2Int> leftTilesInRightSplit = GetLeftFloorTiles(parent.right.area);

                    // Get the overlap between the two above lists
                    bool isConnected = false;
                    var overlappingTiles = GetOverlap(rightTilesInLeftSplit, leftTilesInRightSplit, false, out isConnected);
                    RectIntExclusive pathArea;

                    if (!isConnected)
                    {
                        if (overlappingTiles.Count != 0)
                        {
                            // Pick a random y position within the overlapping area to add a connecting path
                            int randomIndex = UnityEngine.Random.Range(0, overlappingTiles.Count);
                            int randomY = overlappingTiles[randomIndex].tileA.y;
                            pathArea = AddStraightConnectingPath(randomY, parent.left.area, parent.right.area, true);
                            if (animationDelay != 0)
                            {
                                UpdateTiles(pathArea);
                            }
                        }
                        else
                        {
                            var leftTileIndex = UnityEngine.Random.Range(0, rightTilesInLeftSplit.Count);
                            var rightTileIndex = UnityEngine.Random.Range(0, leftTilesInRightSplit.Count);
                            var leftTile = rightTilesInLeftSplit[leftTileIndex];
                            var rightTile = leftTilesInRightSplit[rightTileIndex];
                            AddAngledPath(leftTile, rightTile, false);
                            if (animationDelay != 0)
                            {
                                UpdateTiles(parent.area);
                            }
                        }
                        yield return new WaitForSeconds(animationDelay);
                    }
                }
            }

            if (parent == root)
            {
                List<Vector2Int> leftTiles = GetLeftFloorTiles(parent.area);
                List<Vector2Int> rightTiles = GetRightFloorTiles(parent.area);

                // Get the overlap between the two above lists
                bool isConnected = false;
                var overlappingTiles = GetOverlap(leftTiles, rightTiles, false, out isConnected);
                RectIntExclusive pathArea;

                int numberOfWrappedPaths = Random.Range(1, 4);
                for (int i = 0; i < numberOfWrappedPaths; i++)
                {
                    if (overlappingTiles.Count != 0)
                    {
                        // Pick a random y position within the overlapping area to add a connecting path
                        int randomIndex = Random.Range(0, overlappingTiles.Count);
                        int randomY = overlappingTiles[randomIndex].tileA.y;
                        pathArea = AddStraightConnectingPath(randomY, parent.left.area, parent.right.area, true);
                        if (animationDelay != 0)
                        {
                            UpdateTiles(pathArea);
                        }
                    }
                    else
                    {
                        var leftTileIndex = Random.Range(0, leftTiles.Count);
                        var rightTileIndex = Random.Range(0, rightTiles.Count);
                        var leftTile = leftTiles[leftTileIndex];
                        var rightTile = rightTiles[rightTileIndex];
                        AddAngledPath(leftTile, rightTile, false);
                        if (animationDelay != 0)
                        {
                            UpdateTiles(parent.area);
                        }
                    }
                    yield return new WaitForSeconds(animationDelay);
                }
            }
        }

        private void AddAngledPath(Vector2Int start, Vector2Int end, bool isVertical)
        {
            var map = Map.instance;
            tiles[start.y][start.x] = TileType.FLOOR;
            tiles[end.y][end.x] = TileType.FLOOR;
            Vector2Int dif = (end - start);
            int count = 0;
            while (start != end && count < 40)
            {
                count++;
                if (Mathf.Abs(dif.x) > Mathf.Abs(dif.y))
                {
                    start.x += (int)Mathf.Sign(dif.x);
                }
                else
                {
                    start.y += (int)Mathf.Sign(dif.y);
                }

                if (start == end) break;

                if (start.x >= 0 && start.x < map.width && start.y >= 0 && start.y < map.height)
                {
                    tiles[start.y][start.x] = TileType.FLOOR;
                }
                dif = (end - start);
                if (Mathf.Abs(dif.x) > Mathf.Abs(dif.y))
                {
                    end.x -= (int)Mathf.Sign(dif.x);
                }
                else
                {
                    end.y -= (int)Mathf.Sign(dif.y);
                }

                if (start == end) break;

                if (end.x >= 0 && end.x < map.width && end.y >= 0 && end.y < map.height)
                {
                    tiles[end.y][end.x] = TileType.FLOOR;
                }
                dif = (end - start);
            }
        }

        // Add a floor tile that is part of a straight path between two rooms.
        // This works for vertical or horizontal paths by allowing the coordinates to be inverted
        // Returns true signify that the path should end if the tile is already a floor tile, 
        // or if either neighbor perpendicular to the path is a floor tile
        bool AddStraightPathTile(int i, int j, RectIntExclusive area, bool invert)
        {
            bool isFloor = GetTile(i, j, invert) == TileType.FLOOR;
            bool isLesserNeighborFloor = i - 1 > area.Min(!invert) && GetTile(i - 1, j, invert) == TileType.FLOOR;
            bool isGreaterNeighborFloor = i + 1 < area.Max(!invert) && GetTile(i + 1, j, invert) == TileType.FLOOR;
            bool isLesserLesserNeighborFloor = i - 1 - 1 > area.Min(!invert) && GetTile(i - 1 - 1, j, invert) == TileType.FLOOR;
            bool isGreaterGreaterNeighborFloor = i + 1 + 1 < area.Max(!invert) && GetTile(i + 1 + 1, j, invert) == TileType.FLOOR;

            SetTile(i, j, TileType.FLOOR, invert);

            return isFloor || isLesserNeighborFloor || isGreaterNeighborFloor || isLesserLesserNeighborFloor || isGreaterGreaterNeighborFloor;
        }

        // Create a straight (vertical or horizontal) path starting from the edge of an area, moving inwards until
        // either a floor tile is reached or a the path runs adjacent to a floor tile
        int CreateStraightPath(int i, int startOfPath, int endOfPath, RectIntExclusive area, bool invert)
        {
            int j = startOfPath;
            int incr = endOfPath > startOfPath ? 1 : -1;
            for (; j != endOfPath; j += incr)
            {
                bool pathEnded = AddStraightPathTile(i, j, area, invert);
                if (pathEnded) break;
            }

            return j;
        }

        // Add a straight (vertical or horizontal) connecting path between two rooms
        // This works for vertical or horizontal paths by allowing the coordinates to be inverted
        // Each end of the path terminates when it reaches a floor tile or runs adjacent to a floor tile
        // A door is placed at each end of the path if appropriate
        private RectIntExclusive AddStraightConnectingPath(int i, RectIntExclusive areaA, RectIntExclusive areaB, bool invert)
        {
            int startOfPath = CreateStraightPath(i, areaA.Max(invert), areaA.Min(invert), areaA, invert);
            int endOfPath = CreateStraightPath(i, areaB.Min(invert), areaB.Max(invert), areaB, invert);

            if (IsDoorSpot(i, startOfPath + 1, invert)) SetTile(i, startOfPath + 1, TileType.DOOR, invert);
            if (IsDoorSpot(i - 1, startOfPath, invert)) SetTile(i - 1, startOfPath, TileType.DOOR, invert);
            if (IsDoorSpot(i + 1, startOfPath, invert)) SetTile(i + 1, startOfPath, TileType.DOOR, invert);
            if (IsDoorSpot(i, endOfPath - 1, invert)) SetTile(i, endOfPath - 1, TileType.DOOR, invert);
            if (IsDoorSpot(i - 1, endOfPath, invert)) SetTile(i - 1, endOfPath, TileType.DOOR, invert);
            if (IsDoorSpot(i + 1, endOfPath, invert)) SetTile(i + 1, endOfPath, TileType.DOOR, invert);

            var pathArea = new RectIntExclusive();
            if (invert) pathArea.SetMinMax(startOfPath - 1, endOfPath + 1, i - 1, i + 1);
            else pathArea.SetMinMax(i - 1, i + 1, startOfPath - 1, endOfPath + 1);

            return pathArea;
        }

        void SetTile(int x, int y, TileType value, bool inverted = false)
        {
            if (inverted) tiles[x][Map.instance.WrapX(y)] = value;
            else tiles[y][Map.instance.WrapX(x)] = value;
        }

        TileType GetTile(int x, int y, bool inverted = false)
        {
            if (inverted) return tiles[x][Map.instance.WrapX(y)];
            else return tiles[y][Map.instance.WrapX(x)];
        }

        private List<TilePair> GetOverlap(List<Vector2Int> tilesA, List<Vector2Int> tilesB, bool isVertical, out bool isConnected)
        {
            var overlap = new List<TilePair>();
            isConnected = false;
            foreach (var tileA in tilesA)
            {
                foreach (var tileB in tilesB)
                {
                    if (tileA.x == tileB.x && isVertical)
                    {
                        var pair = new TilePair(tileA, tileB);
                        overlap.Add(pair);
                        if (tileA.y == tileB.y - 1 || tileA.y == tileB.y + 1)
                        {
                            isConnected = true;
                        }
                    }
                    else if (tileA.y == tileB.y && !isVertical)
                    {
                        var pair = new TilePair(tileA, tileB);
                        overlap.Add(pair);
                        if (tileA.x == tileB.x - 1 || tileA.x == tileB.x + 1)
                        {
                            isConnected = true;
                        }
                    }
                }
            }

            return overlap;
        }

        List<Vector2Int> GetTopFloorTiles(RectIntExclusive area)
        {
            List<Vector2Int> floorTiles = new List<Vector2Int>();
            for (int x = area.xMin; x <= area.xMax; x++)
            {
                for (int y = area.yMax; y >= area.yMin; y--)
                {
                    if (tiles[y][x] == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tiles[y][x] == TileType.DOOR)
                    {
                        break;
                    }
                }
            }
            return floorTiles;
        }

        List<Vector2Int> GetBottomFloorTiles(RectIntExclusive area)
        {
            List<Vector2Int> floorTiles = new List<Vector2Int>();
            for (int x = area.xMin; x <= area.xMax; x++)
            {
                for (int y = area.yMin; y <= area.yMax; y++)
                {
                    if (tiles[y][x] == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tiles[y][x] == TileType.DOOR)
                    {
                        break;
                    }
                }
            }
            return floorTiles;
        }

        List<Vector2Int> GetRightFloorTiles(RectIntExclusive area)
        {
            List<Vector2Int> floorTiles = new List<Vector2Int>();
            for (int y = area.yMin; y <= area.yMax; y++)
            {
                for (int x = area.xMax; x >= area.xMin; x--)
                {
                    if (tiles[y][x] == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tiles[y][x] == TileType.DOOR)
                    {
                        break;
                    }
                }
            }
            return floorTiles;
        }

        List<Vector2Int> GetLeftFloorTiles(RectIntExclusive area)
        {
            List<Vector2Int> floorTiles = new List<Vector2Int>();
            for (int y = area.yMin; y <= area.yMax; y++)
            {
                for (int x = area.xMin; x <= area.xMax; x++)
                {
                    if (tiles[y][x] == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tiles[y][x] == TileType.DOOR)
                    {
                        break;
                    }
                }
            }
            return floorTiles;
        }

        IEnumerator GenerateAreas(Node parent, float splitProbability)
        {
            if (UnityEngine.Random.value > splitProbability) yield break;

            bool horizontalHasRoom = parent.area.width > (minBSPArea * 2);
            bool verticalHasRoom = parent.area.height > (minBSPArea * 2);
            if (!horizontalHasRoom && !verticalHasRoom) yield break;

            bool splitHorizontal;
            if (!verticalHasRoom)
            {
                splitHorizontal = true;
            }
            else if (!horizontalHasRoom)
            {
                splitHorizontal = false;
            }
            else
            {
                if (parent.area.width > parent.area.height) splitHorizontal = true;
                else if (parent.area.height > parent.area.width) splitHorizontal = false;
                else if (UnityEngine.Random.value > .5f) splitHorizontal = false;
                else splitHorizontal = true;
            }

            if (splitHorizontal)
            {
                int splitX = UnityEngine.Random.Range(parent.area.xMin + minBSPArea, parent.area.xMax - minBSPArea);
                RectIntExclusive newArea = new RectIntExclusive();
                newArea.xMin = parent.area.xMin;
                newArea.xMax = splitX;
                newArea.yMin = parent.area.yMin;
                newArea.yMax = parent.area.yMax;
                Node child1 = new Node();
                child1.area = newArea;
                child1.parent = parent;
                parent.left = child1;
                yield return new WaitForSeconds(animationDelay);
                yield return Map.instance.StartCoroutine(GenerateAreas(child1, splitProbability * .8f));

                newArea = new RectIntExclusive();
                newArea.xMin = splitX + 1;
                newArea.xMax = parent.area.xMax;
                newArea.yMin = parent.area.yMin;
                newArea.yMax = parent.area.yMax;
                Node child2 = new Node();
                child2.area = newArea;
                child2.parent = parent;
                parent.right = child2;
                yield return new WaitForSeconds(animationDelay);
                yield return Map.instance.StartCoroutine(GenerateAreas(child2, splitProbability * .8f));
            }
            else
            {
                int splitY = UnityEngine.Random.Range(parent.area.yMin + minBSPArea, parent.area.yMax - minBSPArea);
                RectIntExclusive newArea = new RectIntExclusive();
                newArea.xMin = parent.area.xMin;
                newArea.xMax = parent.area.xMax;
                newArea.yMin = parent.area.yMin;
                newArea.yMax = splitY;
                Node child1 = new Node();
                child1.area = newArea;
                child1.parent = parent;
                parent.left = child1;
                yield return new WaitForSeconds(animationDelay);
                yield return Map.instance.StartCoroutine(GenerateAreas(child1, splitProbability * .8f));

                newArea = new RectIntExclusive();
                newArea.xMin = parent.area.xMin;
                newArea.xMax = parent.area.xMax;
                newArea.yMin = splitY + 1;
                newArea.yMax = parent.area.yMax;
                Node child2 = new Node();
                child2.area = newArea;
                child2.parent = parent;
                parent.right = child2;
                yield return new WaitForSeconds(animationDelay);
                yield return Map.instance.StartCoroutine(GenerateAreas(child2, splitProbability * .8f));
            }
        }

        public void UpdateTiles(RectIntExclusive area)
        {
            var map = Map.instance;
            for (int y = area.yMin; y <= area.yMax; y++)
            {
                for (int x = area.xMin; x <= area.xMax; x++)
                {
                    AddTileObjects(map, x, y);
                }
            }
        }

        public void SpawnFinalDoor(Map map, RectIntExclusive area)
        {
            var spawnArea = new RectIntExclusive();
            spawnArea.xMin = 0;
            spawnArea.xMax = area.xMax;
            spawnArea.yMin = area.yMax - 1;
            spawnArea.yMax = area.yMax;
            var wallTiles = map.GetTilesOfType("Wall", spawnArea);
            for (int i = wallTiles.Count - 1; i >= 0; i--)
            {
                var tile = wallTiles[i];
                var adjacentTile = Map.instance.tileObjects[tile.y - 1][tile.x];

                if (!adjacentTile.ContainsObjectOfType("Floor") || adjacentTile.ContainsObjectOfType("Wall"))
                {
                    wallTiles.RemoveAt(i);
                }
            }
            int r = UnityEngine.Random.Range(0, wallTiles.Count);
            var tileToSpawnDoorOn = wallTiles[r];
            for (var node = tileToSpawnDoorOn.objectList.First; node != null;)
            {
                var next = node.Next;
                if (node.Value.objectName == "Wall") tileToSpawnDoorOn.RemoveObject(node.Value, true);
                node = next;
            }
            tileToSpawnDoorOn.SpawnAndAddObject(finalDoorPrefab);
        }

        void AddTileObjects(Map map, int x, int y)
        {
            map.tileObjects[y][x].RemoveAllObjects();
            if (tiles[y][x] == TileType.NOTHING)
            {
                DungeonObject ob = GetRandomBaseTile(TileType.NOTHING);
                map.tileObjects[y][x].SpawnAndAddObject(ob);
            }
            else
            {
                // Pick a floor tile based on spawn rates
                DungeonObject ob = GetRandomBaseTile(TileType.FLOOR);
                map.tileObjects[y][x].SpawnAndAddObject(ob);
                if (tiles[y][x] != TileType.FLOOR)
                {
                    var type = tiles[y][x];
                    ob = GetRandomBaseTile(type);
                    var instanceOb = map.tileObjects[y][x].SpawnAndAddObject(ob);

                    if (type == TileType.DOOR)
                    {
                        Direction orientation = Direction.UP;
                        if (y > 0 && tiles[y - 1][x] == TileType.WALL && y < tiles.Length - 1 && tiles[y + 1][x] == TileType.WALL)
                        {
                            orientation = Direction.RIGHT;
                        }
                        foreach (var glyph in instanceOb.GetComponentsInChildren<OrientedGlyph>(true))
                        {
                            glyph.orientation = orientation;
                        }
                    }
                }
            }
        }

        public BiomeDropRate[] GetSpawnRatesForBaseType(TileType baseType)
        {
            switch (baseType)
            {
                case TileType.NOTHING: return nothings;
                case TileType.FLOOR: return floors;
                case TileType.DOOR: return doors;
                case TileType.WALL: return walls;
                case TileType.TORCH: return torches;
                default:
                    Debug.LogError("Invalid base tile type: " + baseType.ToString());
                    return null;
            }
        }

        public DungeonObject GetRandomBaseTile(TileType baseType)
        {
            // Gather all possible floor tiles from each biome at this location
            var possibleObs = GetSpawnRatesForBaseType(baseType);
            return Biome.SelectRandomObject(possibleObs);
        }
    }
}
