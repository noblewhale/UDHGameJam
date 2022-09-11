namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Random = UnityEngine.Random;

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
        public DungeonObject torchPrefab;
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
            NOTHING, FLOOR, WALL, DOOR, ELECTRIC
        }

        public struct TileTemplate
        {
            public TileType type;
            public bool isRoomTile;
        }

        public TileTemplate[][] tileTemplates;

        override public IEnumerator PreProcessMap(Map map, BiomeObject biomeObject)
        {
            Debug.Log("Preprocess level 1");

            tileTemplates = new TileTemplate[map.height][];
            for (int y = 0; y < map.height; y++) tileTemplates[y] = new TileTemplate[map.width];

            //var itemsBiome = Instantiate(items);
            //itemsBiome.area = area;
            //subBiomes.Add(itemsBiome);

            yield return new WaitForSeconds(animationDelay * 2);
            root = new Node();
            root.area = biomeObject.area;
            yield return new WaitForSeconds(animationDelay);
            yield return Map.instance.StartCoroutine(GenerateAreas(root, 1, biomeObject));
            yield return Map.instance.StartCoroutine(GenerateRooms(root, biomeObject));
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

            base.PreProcessMap(map, biomeObject);
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
                    if (tileTemplates[y][x].type == TileType.DOOR)
                    {
                        if (HasFloorOnThreeSides(x, y)) tileTemplates[y][x].type = TileType.FLOOR;
                    }
                }
            }
        }

        bool HasFloorOnThreeSides(int x, int y)
        {
            var map = Map.instance;
            if (y <= 0 || y >= map.height - 1) return false;
            int floorCount = 0;
            int wrappedXLeft = map.GetXTilePositionOnMap(x - 1);
            int wrappedXRight = map.GetXTilePositionOnMap(x + 1);
            if (tileTemplates[y][wrappedXLeft].type == TileType.FLOOR) floorCount++;
            if (tileTemplates[y][wrappedXRight].type == TileType.FLOOR) floorCount++;
            if (tileTemplates[y - 1][x].type == TileType.FLOOR) floorCount++;
            if (tileTemplates[y + 1][x].type == TileType.FLOOR) floorCount++;

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

            int wrappedX = map.GetXTilePositionOnMap(x);
            int wrappedXLeft = map.GetXTilePositionOnMap(x - 1);
            int wrappedXRight = map.GetXTilePositionOnMap(x + 1);
            if (tileTemplates[y][wrappedXLeft].type == TileType.FLOOR && tileTemplates[y][wrappedXRight].type == TileType.FLOOR)
            {
                if (tileTemplates[y - 1][wrappedX].type == TileType.NOTHING && tileTemplates[y + 1][wrappedX].type == TileType.NOTHING)
                {
                    if (tileTemplates[y - 1][wrappedXLeft].type == TileType.FLOOR || tileTemplates[y + 1][wrappedXLeft].type == TileType.FLOOR || 
                        tileTemplates[y - 1][wrappedXRight].type == TileType.FLOOR || tileTemplates[y + 1][wrappedXRight].type == TileType.FLOOR)
                    {
                        return true;
                    }
                }
            }
            if (tileTemplates[y - 1][wrappedX].type == TileType.FLOOR && tileTemplates[y + 1][wrappedX].type == TileType.FLOOR)
            {
                if (tileTemplates[y][wrappedXLeft].type == TileType.NOTHING && tileTemplates[y][wrappedXRight].type == TileType.NOTHING)
                {
                    if (tileTemplates[y - 1][wrappedXLeft].type == TileType.FLOOR || tileTemplates[y + 1][wrappedXLeft].type == TileType.FLOOR ||
                        tileTemplates[y - 1][wrappedXRight].type == TileType.FLOOR || tileTemplates[y + 1][wrappedXRight].type == TileType.FLOOR)
                    {
                        return true;
                    }
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
                    if (tileTemplates[y][x].type == TileType.NOTHING)
                    {
                        if (HasAdjacentFloor(x, y)) tileTemplates[y][x].type = TileType.WALL;
                    }
                }
            }
        }
        bool HasAdjacentFloor(int x, int y)
        {
            var map = Map.instance;
            int wrappedX = map.GetXTilePositionOnMap(x - 1);
            if (tileTemplates[y][wrappedX].type == TileType.FLOOR) return true;
            wrappedX = map.GetXTilePositionOnMap(x + 1);
            if (tileTemplates[y][wrappedX].type == TileType.FLOOR) return true;
            if (y > 0)
            {
                wrappedX = map.GetXTilePositionOnMap(x - 1);
                if (tileTemplates[y - 1][wrappedX].type == TileType.FLOOR) return true;
                wrappedX = map.GetXTilePositionOnMap(x + 1);
                if (tileTemplates[y - 1][wrappedX].type == TileType.FLOOR) return true;
                if (tileTemplates[y - 1][x].type == TileType.FLOOR) return true;
            }
            if (y < map.height - 1)
            {
                wrappedX = map.GetXTilePositionOnMap(x - 1);
                if (tileTemplates[y + 1][wrappedX].type == TileType.FLOOR) return true;
                wrappedX = map.GetXTilePositionOnMap(x + 1);
                if (tileTemplates[y + 1][wrappedX].type == TileType.FLOOR) return true;
                if (tileTemplates[y + 1][x].type == TileType.FLOOR) return true;
            }

            return false;
        }

        IEnumerator GenerateRooms(Node parent, BiomeObject biomeObject)
        {
            if (parent == null) yield break;

            if (parent.left == null && parent.right == null)
            {
                // Leaf node, actually generate a room
                int maxWidth = parent.area.width - 2;
                int maxHeight = parent.area.height - 2;
                int minWidth = Mathf.Max(2, maxWidth / 3);
                int minHeight = Mathf.Max(2, maxHeight / 3);
                int w = Random.Range(minWidth, maxWidth);
                int h = Random.Range(minHeight, maxHeight);
                int xMin = Random.Range(parent.area.xMin + 1, parent.area.xMax - w - 1);
                int yMin = Random.Range(parent.area.yMin + 1, parent.area.yMax - h - 1);
                var rect = new RectIntExclusive(xMin, yMin, w, h);
                parent.room = rect;

                try
                {
                    var topLeftCorner = Map.instance.GetTilePositionOnMap(new Vector2Int(rect.xMin - 1, rect.yMin - 1));
                    var topRightCorner = Map.instance.GetTilePositionOnMap(new Vector2Int(rect.xMax + 1, rect.yMin - 1));
                    for (int x = topLeftCorner.x; x <= topRightCorner.x; x++)
                    {
                        tileTemplates[topLeftCorner.y][x].isRoomTile = true;
                    }
                    for (int y = rect.yMin; y <= rect.yMax; y++)
                    {
                        var p = Map.instance.GetTilePositionOnMap(new Vector2Int(rect.xMin - 1, y));
                        tileTemplates[p.y][p.x].isRoomTile = true;
                        for (int x = rect.xMin; x <= rect.xMax; x++)
                        {
                            tileTemplates[y][x].type = TileType.FLOOR;
                            tileTemplates[y][x].isRoomTile = true;
                        }
                        p = Map.instance.GetTilePositionOnMap(new Vector2Int(rect.xMax + 1, y));
                        tileTemplates[p.y][p.x].isRoomTile = true;
                    }
                    var bottomLeftCorner = Map.instance.GetTilePositionOnMap(new Vector2Int(rect.xMin - 1, rect.yMax + 1));
                    var bottomRightCorner = Map.instance.GetTilePositionOnMap(new Vector2Int(rect.xMax + 1, rect.yMax + 1));
                    for (int x = bottomLeftCorner.x; x <= bottomRightCorner.x; x++)
                    {
                        tileTemplates[bottomLeftCorner.y][x].isRoomTile = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

                var roomCreaturesObject = Instantiate(Map.instance.biomeObjectPrefab, biomeObject.transform).GetComponent<BiomeObject>();
                var roomCreatures = Instantiate(scorpions);
                roomCreaturesObject.area = rect;
                roomCreaturesObject.biome = roomCreatures;
                subBiomes.Add(roomCreatures);
                biomeObject.subBiomes.Add(roomCreaturesObject);

                var roomTrapsObject = Instantiate(Map.instance.biomeObjectPrefab, biomeObject.transform).GetComponent<BiomeObject>();
                var roomTraps = Instantiate(electricTraps);
                roomTrapsObject.area = new RectIntExclusive(rect.xMin + 1, rect.yMin + 1, rect.width - 1, rect.height - 1);
                roomTrapsObject.biome = roomTraps;
                subBiomes.Add(roomTraps);
                biomeObject.subBiomes.Add(roomTrapsObject);

                if (animationDelay != 0)
                {
                    UpdateTiles(parent.room);
                    yield return new WaitForSeconds(animationDelay);
                }
            }
            else
            {
                // Not a leaf node, keep traversing the tree
                yield return Map.instance.StartCoroutine(GenerateRooms(parent.left, biomeObject));
                yield return Map.instance.StartCoroutine(GenerateRooms(parent.right, biomeObject));

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
                            int randomIndex = Random.Range(0, overlappingTiles.Count);
                            int randomX = overlappingTiles[randomIndex].tileA.x;
                            pathArea = AddStraightConnectingPath(randomX, parent.left.area, parent.right.area, false);
                            if (animationDelay != 0)
                            {
                                UpdateTiles(pathArea);
                            }
                        }
                        else
                        {
                            var bottomTileIndex = Random.Range(0, topTilesInBottomSplit.Count);
                            var topTileIndex = Random.Range(0, bottomTilesInTopSplit.Count);
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
                            var leftTileIndex = Random.Range(0, rightTilesInLeftSplit.Count);
                            var rightTileIndex = Random.Range(0, leftTilesInRightSplit.Count);
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

            //if (parent == root)
            //{
            //    List<Vector2Int> leftTiles = GetLeftFloorTiles(parent.area);
            //    List<Vector2Int> rightTiles = GetRightFloorTiles(parent.area);

            //    // Get the overlap between the two above lists
            //    bool isConnected = false;
            //    var overlappingTiles = GetOverlap(leftTiles, rightTiles, false, out isConnected);
            //    RectIntExclusive pathArea;

            //    int numberOfWrappedPaths = Random.Range(1, 4);
            //    for (int i = 0; i < numberOfWrappedPaths; i++)
            //    {
            //        if (overlappingTiles.Count != 0)
            //        {
            //            // Pick a random y position within the overlapping area to add a connecting path
            //            int randomIndex = Random.Range(0, overlappingTiles.Count);
            //            int randomY = overlappingTiles[randomIndex].tileA.y;
            //            pathArea = AddStraightConnectingPath(randomY, parent.area, parent.area, true);
            //            if (animationDelay != 0)
            //            {
            //                UpdateTiles(pathArea);
            //            }
            //        }
            //        else
            //        {
            //            var leftTileIndex = Random.Range(0, leftTiles.Count);
            //            var rightTileIndex = Random.Range(0, rightTiles.Count);
            //            var leftTile = leftTiles[leftTileIndex];
            //            var rightTile = rightTiles[rightTileIndex];
            //            AddAngledPath(leftTile, rightTile, false);
            //            if (animationDelay != 0)
            //            {
            //                UpdateTiles(parent.area);
            //            }
            //        }
            //        yield return new WaitForSeconds(animationDelay);
            //    }
            //}
        }

        private void AddAngledPath(Vector2Int start, Vector2Int end, bool isVertical)
        {
            var map = Map.instance;
            tileTemplates[start.y][start.x].type = TileType.FLOOR;
            tileTemplates[end.y][end.x].type = TileType.FLOOR;
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
                    tileTemplates[start.y][start.x].type = TileType.FLOOR;
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
                    tileTemplates[end.y][end.x].type = TileType.FLOOR;
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
            bool isFloor = GetTile(i, j, invert).type == TileType.FLOOR;
            bool isLesserNeighborFloor = i - 1 >= area.Min(!invert) && GetTile(i - 1, j, invert).type == TileType.FLOOR;
            bool isGreaterNeighborFloor = i + 1 <= area.Max(!invert) && GetTile(i + 1, j, invert).type == TileType.FLOOR;
            bool isLesserLesserNeighborFloor = i - 1 - 1 >= area.Min(!invert) && GetTile(i - 1 - 1, j, invert).type == TileType.FLOOR;
            bool isGreaterGreaterNeighborFloor = i + 1 + 1 <= area.Max(!invert) && GetTile(i + 1 + 1, j, invert).type == TileType.FLOOR;

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
            int startOfPath = CreateStraightPath(i, areaA.Max(invert), areaA.Min(invert) - 1, areaA, invert);
            int endOfPath = CreateStraightPath(i, areaB.Min(invert), areaB.Max(invert) + 1, areaB, invert);

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
            if (inverted) tileTemplates[x][Map.instance.GetXTilePositionOnMap(y)].type = value;
            else tileTemplates[y][Map.instance.GetXTilePositionOnMap(x)].type = value;
        }

        TileTemplate GetTile(int x, int y, bool inverted = false)
        {
            if (inverted) return tileTemplates[x][Map.instance.GetXTilePositionOnMap(y)];
            else return tileTemplates[y][Map.instance.GetXTilePositionOnMap(x)];
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
                    if (tileTemplates[y][x].type == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tileTemplates[y][x].type == TileType.DOOR)
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
                    if (tileTemplates[y][x].type == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tileTemplates[y][x].type == TileType.DOOR)
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
                    if (tileTemplates[y][x].type == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tileTemplates[y][x].type == TileType.DOOR)
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
                    if (tileTemplates[y][x].type == TileType.FLOOR)
                    {
                        floorTiles.Add(new Vector2Int(x, y));
                        break;
                    }
                    else if (tileTemplates[y][x].type == TileType.DOOR)
                    {
                        break;
                    }
                }
            }
            return floorTiles;
        }

        IEnumerator GenerateAreas(Node parent, float splitProbability, BiomeObject biomeObject)
        {
            if (Random.value > splitProbability) yield break;

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
                else if (Random.value > .5f) splitHorizontal = false;
                else splitHorizontal = true;
            }

            if (splitHorizontal)
            {
                int splitX = Random.Range(parent.area.xMin + minBSPArea, parent.area.xMax - minBSPArea);
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
                yield return Map.instance.StartCoroutine(GenerateAreas(child1, splitProbability * .8f, biomeObject));

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
                yield return Map.instance.StartCoroutine(GenerateAreas(child2, splitProbability * .8f, biomeObject));
            }
            else
            {
                int splitY = Random.Range(parent.area.yMin + minBSPArea, parent.area.yMax - minBSPArea);
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
                yield return Map.instance.StartCoroutine(GenerateAreas(child1, splitProbability * .8f, biomeObject));

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
                yield return Map.instance.StartCoroutine(GenerateAreas(child2, splitProbability * .8f, biomeObject));
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
                var adjacentTile = Map.instance.GetTile(tile.position + Vector2Int.up);

                if (!adjacentTile.ContainsObjectOfType("Floor") || adjacentTile.ContainsObjectOfType("Wall"))
                {
                    wallTiles.RemoveAt(i);
                }
            }
            int r = Random.Range(0, wallTiles.Count);
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
            //map.tileObjects[y][x].isAlwaysLit = true;

            map.tileObjects[y][x].RemoveAllObjects();
            if (tileTemplates[y][x].type == TileType.NOTHING)
            {
                DungeonObject ob = GetRandomBaseTile(TileType.NOTHING);
                map.tileObjects[y][x].SpawnAndAddObject(ob, 1, 1);
            }
            else
            {
                // Pick a floor tile based on spawn rates
                DungeonObject ob = GetRandomBaseTile(TileType.FLOOR);
                map.tileObjects[y][x].SpawnAndAddObject(ob, 1, 1);

                if (tileTemplates[y][x].isRoomTile)
                {
                    // Add a torch so rooms are always lit
                    map.tileObjects[y][x].SpawnAndAddObject(torchPrefab);
                }

                if (tileTemplates[y][x].type != TileType.FLOOR)
                {
                    var tileTemplate = tileTemplates[y][x];
                    ob = GetRandomBaseTile(tileTemplate.type);
                    var instanceOb = map.tileObjects[y][x].SpawnAndAddObject(ob, 1, 2);
                    //instanceOb.isAlwaysLit = true;
                    //instanceOb.isVisibleWhenNotInSight = true;
                    //instanceOb.hasBeenSeen = true;
                    if (tileTemplate.type == TileType.DOOR)
                    {
                        Direction orientation = Direction.UP;
                        if (y > 0 && tileTemplates[y - 1][x].type == TileType.WALL && y < tileTemplates.Length - 1 && tileTemplates[y + 1][x].type == TileType.WALL)
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

            //map.tileObjects[y][x].SetLit(true);
        }

        public BiomeDropRate[] GetSpawnRatesForBaseType(TileType baseType)
        {
            switch (baseType)
            {
                case TileType.NOTHING: return nothings;
                case TileType.FLOOR: return floors;
                case TileType.DOOR: return doors;
                case TileType.WALL: return walls;
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
