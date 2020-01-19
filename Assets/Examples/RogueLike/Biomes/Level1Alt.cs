using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Level1Alt : BiomeType
{
    public int minBSPArea = 4;
    float animationDelay= .15f;

    public BiomeDropRate[] nothings;
    public BiomeDropRate[] floors;
    public BiomeDropRate[] walls;
    public BiomeDropRate[] doors;
    public DungeonObject finalDoorPrefab;


    class Node
    {
        public Node left;
        public Node right;
        public Node parent;
        public RectIntExclusive area;
        public RectIntExclusive room;
        public bool areChildrenConnected;
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
        NOTHING, FLOOR, WALL, DOOR
    }

    public TileType[][] tiles;

    override public void PreProcessMap(Map map, RectIntExclusive area)
    {
        tiles = new TileType[map.height][];
        for (int y = 0; y < map.height; y++) tiles[y] = new TileType[map.width];

        map.StartCoroutine(AsyncMapGeneration(area));
        //GenerateRooms(root, map);
        //AddWalls(map);
        //PruneDoors();
        //UpdateTiles(map, area);
        //SpawnFinalDoor(map, area);
    }

    IEnumerator AsyncMapGeneration(RectIntExclusive area)
    {
        yield return new WaitForSeconds(animationDelay*2);
        root = new Node();
        root.area = area;
        root.area.yMin += 1;
        root.area.yMax -= 1;
        root.area.xMin += 1;
        root.area.xMax -= 1;
        yield return new WaitForSeconds(animationDelay);
        yield return Map.instance.StartCoroutine(GenerateAreas(root, 1));
        yield return Map.instance.StartCoroutine(GenerateRooms(root));
        var mapArea = new RectIntExclusive(0, 0, Map.instance.width, Map.instance.height);
        AddWalls();
        UpdateTiles(mapArea);
        yield return new WaitForSeconds(animationDelay);
        PruneDoors();
        UpdateTiles(mapArea);
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

    bool IsDoorSpot(int x, int y, bool inverted)
    {
        var map = Map.instance;

        if (inverted)
        {
            int t = x;
            x = y;
            y = t;
        }

        if (y <= 0 || y >= map.height - 1) return false;
        
        int wrappedXLeft = map.WrapX(x - 1);
        int wrappedXRight = map.WrapX(x + 1);
        if (tiles[y][wrappedXLeft] == TileType.FLOOR && tiles[y][wrappedXRight] == TileType.FLOOR)
        {
            if (tiles[y - 1][x] == TileType.NOTHING && tiles[y + 1][x] == TileType.NOTHING)
            {
                return true;
            }
        }
        if (tiles[y - 1][x] == TileType.FLOOR && tiles[y + 1][x] == TileType.FLOOR)
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
            int minWidth = Mathf.Max(1, parent.area.width / 3);
            int minHeight = Mathf.Max(1, parent.area.height / 3);
            int w = UnityEngine.Random.Range(minWidth, parent.area.width + 1);
            int h = UnityEngine.Random.Range(minHeight, parent.area.height + 1);
            int xMin = UnityEngine.Random.Range(parent.area.xMin, parent.area.xMax - (w - 1) + 1);
            int yMin = UnityEngine.Random.Range(parent.area.yMin, parent.area.yMax - (h - 1) + 1);
            var rect = new RectIntExclusive(xMin, yMin, w, h);
            parent.room = rect;

            for (int y = rect.yMin; y <= rect.yMax; y++)
            {
                for (int x = rect.xMin; x <= rect.xMax; x++)
                {
                    tiles[y][x] = TileType.FLOOR;
                }
            }
            UpdateTiles(parent.room);
            yield return new WaitForSeconds(animationDelay);
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
                        AddStraightConnectingPath(randomX, parent.left.area, parent.right.area, out pathArea, true);
                        UpdateTiles(pathArea);
                    }
                    else
                    {
                        var bottomTileIndex = UnityEngine.Random.Range(0, topTilesInBottomSplit.Count);
                        var topTileIndex = UnityEngine.Random.Range(0, bottomTilesInTopSplit.Count);
                        var bottomTile = topTilesInBottomSplit[bottomTileIndex];
                        var topTile = bottomTilesInTopSplit[topTileIndex];
                        AddAngledPath(bottomTile, topTile, true);
                        UpdateTiles(parent.area);
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
                        AddStraightConnectingPath(randomY, parent.left.area, parent.right.area, out pathArea, false);
                        UpdateTiles(pathArea);
                    }
                    else
                    {
                        var leftTileIndex = UnityEngine.Random.Range(0, rightTilesInLeftSplit.Count);
                        var rightTileIndex = UnityEngine.Random.Range(0, leftTilesInRightSplit.Count);
                        var leftTile = rightTilesInLeftSplit[leftTileIndex];
                        var rightTile = leftTilesInRightSplit[rightTileIndex];
                        AddAngledPath(leftTile, rightTile, false);
                        UpdateTiles(parent.area);
                    }
                    yield return new WaitForSeconds(animationDelay);
                }
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

    private void AddStraightConnectingPath(int fixedValue, RectIntExclusive areaA, RectIntExclusive areaB, out RectIntExclusive pathArea, bool isVertical)
    {
        int startOfPath = 0;
        int endOfPath = 0;
        int areaAMin, areaAMax;
        int areaBMin, areaBMax;
        if (isVertical)
        {
            areaAMin = areaA.yMin;
            areaAMax = areaA.yMax;
            areaBMin = areaB.yMin;
            areaBMax = areaB.yMax;
        }
        else
        {
            areaAMin = areaA.xMin;
            areaAMax = areaA.xMax;
            areaBMin = areaB.xMin;
            areaBMax = areaB.xMax;
        }

        int fixedLess = fixedValue - 1;
        int fixedGreater = fixedValue + 1;
        if (isVertical)
        {
            fixedLess = Map.instance.WrapX(fixedLess);
            fixedGreater = Map.instance.WrapX(fixedGreater);
        }

        for (int i = areaAMax; i >= areaAMin; i--)
        {
            if (GetTile(fixedValue, i, !isVertical) == TileType.FLOOR)
            {
                startOfPath = i + 1;
                break;
            }
        }
        for (int i = areaBMin; i <= areaBMax; i++)
        {
            if (GetTile(fixedValue, i, !isVertical) == TileType.FLOOR)
            {
                endOfPath = i - 1;
                break;
            }
        }

        if (isVertical)
        {
            Debug.Log("fixed x: " + fixedValue);
        }
        else
        {
            Debug.Log("fixed y: " + fixedValue);
        }
        Debug.Log("areaA: " + areaA);
        Debug.Log("areaB: " + areaB);
        Debug.Log("Start of path: " + startOfPath);
        Debug.Log("End of path: " + endOfPath);

        int fromStartLength = 0;
        int fromEndLength = 0;
        for (int i = startOfPath; i <= endOfPath; i++)
        {
            fromStartLength++;
            if (i > areaAMax) 
            {
                if (GetTile(fixedLess, i, !isVertical) == TileType.FLOOR ||
                    GetTile(fixedGreater, i, !isVertical) == TileType.FLOOR)
                {
                    break;
                }
            }
        }

        for (int i = endOfPath; i >= startOfPath; i--)
        {
            fromEndLength++;
            if (i < areaBMin)
            {
                if (GetTile(fixedLess, i, !isVertical) == TileType.FLOOR ||
                    GetTile(fixedGreater, i, !isVertical) == TileType.FLOOR)
                {
                    break;
                }
            }
        }

        Vector2Int startDoor = new Vector2Int(fixedValue, startOfPath);
        Vector2Int endDoor = new Vector2Int(fixedValue, endOfPath);
        int start = startOfPath;
        int end = startOfPath + (fromStartLength - 1);
        int incr = 1;
        bool reverse = fromEndLength < fromStartLength;
        if (reverse)
        {
            start = endOfPath;
            end = endOfPath - (fromEndLength - 1);
            incr = -1;
        }
        for (int i = start; reverse ? i >= end : i <= end; i += incr)
        {
            if (GetTile(fixedValue, i, !isVertical) == TileType.FLOOR)
            {
                endDoor = new Vector2Int(fixedValue, i - incr);
            }
            else
            {
                SetTile(fixedValue, i, TileType.FLOOR, !isVertical);
                if ((i > areaAMax && !reverse) || (i < areaBMin && reverse))
                {
                    if (GetTile(fixedLess, i, !isVertical) == TileType.FLOOR)
                    {
                        Debug.LogError("The thing happened");
                        endDoor = new Vector2Int(fixedLess, i);
                    }
                    if (GetTile(fixedGreater, i, !isVertical) == TileType.FLOOR)
                    {
                        endDoor = new Vector2Int(fixedGreater, i);
                        Debug.LogError("The thing happened");
                    }
                }
            }
        }
        if (IsDoorSpot(startDoor.x, startDoor.y, !isVertical)) SetTile(startDoor.x, startDoor.y, TileType.DOOR, !isVertical);
        if (IsDoorSpot(endDoor.x, endDoor.y, !isVertical)) SetTile(endDoor.x, endDoor.y, TileType.DOOR, !isVertical);

        pathArea = new RectIntExclusive();
        if (isVertical)
        {
            pathArea.xMin = fixedLess;
            pathArea.xMax = fixedGreater;
            pathArea.yMin = startOfPath - 1;
            pathArea.yMax = endOfPath + 1;
        }
        else
        {
            pathArea.xMin = startOfPath - 1;
            pathArea.xMax = endOfPath + 1;
            pathArea.yMin = fixedLess;
            pathArea.yMax = fixedGreater;
        }
    }

    void SetTile(int x, int y, TileType value, bool inverted)
    {
        if (inverted) tiles[x][y] = value;
        else tiles[y][x] = value;
    }

    TileType GetTile(int x, int y, bool inverted)
    {
        if (inverted) return tiles[x][y];
        else return tiles[y][x];
    }

    //private void AddHorizontalConnectingPath(int y, RectIntExclusive leftArea, RectIntExclusive rightArea, out RectIntExclusive pathArea)
    //{
    //    pathArea = new RectIntExclusive();
    //    pathArea.yMin = pathArea.yMax = y;
    //    pathArea.xMin = rightArea.xMax;
    //    pathArea.xMax = leftArea.xMin;
    //    Vector2Int startOfPath = new Vector2Int(leftArea.xMax, y);
    //    Vector2Int endOfPath = new Vector2Int(rightArea.xMin, y);
    //    for (int x = leftArea.xMax; x >= leftArea.xMin; x--)
    //    {
    //        if (x < pathArea.xMin) pathArea.xMin = x;
    //        if (tiles[y][x] == TileType.FLOOR)
    //        {
    //            startOfPath = new Vector2Int(x + 1, y);
    //            break;
    //        }
    //        tiles[y][x] = TileType.FLOOR;
    //        if (tiles[y - 1][x] == TileType.FLOOR)
    //        {
    //            startOfPath = new Vector2Int(x, y - 1);
    //            break;
    //        }
    //        if (tiles[y + 1][x] == TileType.FLOOR)
    //        {
    //            startOfPath = new Vector2Int(x, y + 1);
    //            break;
    //        }
    //    }
    //    for (int x = rightArea.xMin; x <= rightArea.xMax; x++)
    //    {
    //        if (x > pathArea.xMax) pathArea.xMax = x;
    //        if (tiles[y][x] == TileType.FLOOR)
    //        {
    //            endOfPath = new Vector2Int(x - 1, y);
    //            break;
    //        }
    //        tiles[y][x] = TileType.FLOOR;
    //        if (tiles[y - 1][x] == TileType.FLOOR)
    //        {
    //            endOfPath = new Vector2Int(x, y - 1);
    //            break;
    //        }
    //        if (tiles[y + 1][x] == TileType.FLOOR)
    //        {
    //            endOfPath = new Vector2Int(x, y + 1);
    //            break;
    //        }
    //    }

    //    if (IsDoorSpot(startOfPath.x, startOfPath.y)) tiles[startOfPath.y][startOfPath.x] = TileType.DOOR;
    //    if (IsDoorSpot(endOfPath.x, endOfPath.y)) tiles[endOfPath.y][endOfPath.x] = TileType.DOOR;
    //}

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

        bool horizontalHasRoom = parent.area.width > minBSPArea * 2;
        bool verticalHasRoom = parent.area.height > minBSPArea * 2;
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
                map.tileObjects[y][x].SpawnAndAddObject(ob);
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
