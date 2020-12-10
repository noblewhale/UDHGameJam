using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Level1 : BiomeType
{
    public BiomeDropRate[] nothings;
    public BiomeDropRate[] floors;
    public BiomeDropRate[] walls;
    public BiomeDropRate[] doors;
    public DungeonObject finalDoorPrefab;

    List<Tile> wallTiles = new List<Tile>();

    public enum TileType
    {
        NOTHING, FLOOR, WALL, DOOR
    }

    public TileType[][] tiles;

    override public IEnumerator PreProcessMap(Map map, Biome biome)
    {
        tiles = new TileType[map.height][];
        for (int y = 0; y < map.height; y++) tiles[y] = new TileType[map.width];

        wallTiles.Clear();

        GenerateRooms(map, biome.area, 30);
        ConnectRooms(map);
        UpdateTiles(map, biome.area);
        SpawnFinalDoor(map, biome.area);

        yield break;
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

    public void UpdateTiles(Map map, RectIntExclusive area)
    {
        for (int y = area.yMin; y <= area.yMax; y++)
        {
            for (int x = area.xMin; x <= area.xMax; x++)
            {
                AddTileObjects(map, x, y);
            }
        }
    }

    void AddTileObjects(Map map, int x, int y)
    {
        //map.tileObjects[y][x].DestroyAllObjects();
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

    void GenerateRooms(Map map, RectIntExclusive area, int numRooms)
    {
        for (int n = 0; n < numRooms; n++)
        {
            int w = Random.Range(3, map.width / 3);
            int h = Random.Range(3, map.height / 3);
            int x = Random.Range(0, map.width);
            int y = Random.Range(0, map.height - 2);

            GenerateRoom(map, x, y, w, h);
        }
    }

    void GenerateRoom(Map map, int x, int y, int w, int h)
    {
        for (int yi = 0; yi < h; yi++)
        {
            for (int xi = 0; xi < w; xi++)
            {
                if (y + yi >= map.height) continue;

                int wrappedX = x + xi;
                if (wrappedX >= map.width) wrappedX = wrappedX - map.width;

                if (yi == 0 || yi == h - 1 || xi == 0 || xi == w - 1 || y + yi == map.height - 1)
                {
                    if (tiles[y + yi][wrappedX] != TileType.FLOOR)
                    {
                        tiles[y + yi][wrappedX] = TileType.WALL;
                    }
                }
                else
                {
                    tiles[y + yi][wrappedX] = TileType.FLOOR;
                }
                //AddTileObjects(map, wrappedX, y + yi);
            }
        }
    }

    void ConnectRooms(Map map)
    {
        int floodX = 0, floodY = 0;
        bool floorTileFound = false;
        do
        {
            floorTileFound = false;
            for (floodY = 1; floodY < map.height - 1; floodY++)
            {
                for (floodX = 0; floodX < map.width; floodX++)
                {
                    if (tiles[floodY][floodX] == TileType.FLOOR && !map.tileObjects[floodY][floodX].isFloodFilled)
                    {
                        floorTileFound = true;
                        break;
                    }
                }
                if (floorTileFound) break;
            }

            if (floorTileFound)
            {
                bool foundPath = true;

                while (foundPath)
                {
                    FloodFill(map, floodX, floodY);

                    foundPath = CreateConnectingPath(map, ref floodX, ref floodY);
                }
            }
        } while (floorTileFound);
    }

    void FloodFill(Map map, int floodX, int floodY)
    {
        if (floodY < 0 || floodY >= map.height) return;

        int wrappedX = floodX;
        if (wrappedX < 0) wrappedX = map.width + wrappedX;
        else if (wrappedX >= map.width) wrappedX = wrappedX - map.width;

        if (map.tileObjects[floodY][wrappedX].isFloodFilled) return;

        if (tiles[floodY][wrappedX] != TileType.WALL)
        {
            map.tileObjects[floodY][wrappedX].isFloodFilled = true;
            //map.tileObjects[floodY][wrappedX].objectList.First.Value.glyphs[0].color = Color.green;
        }
        else
        {
            if (tiles[floodY][wrappedX] == TileType.WALL)
            {
                //map.tileObjects[floodY][wrappedX].objectList.First.Value.glyphs[0].color = Color.red;
                wallTiles.Add(map.tileObjects[floodY][wrappedX]);
            }
            return;
        }

        FloodFill(map, wrappedX - 1, floodY);
        FloodFill(map, wrappedX + 1, floodY);
        FloodFill(map, wrappedX, floodY - 1);
        FloodFill(map, wrappedX, floodY + 1);
    }

    Tile FindFloorTile(Map map, Direction dir, int x, int y, out int distance)
    {
        int startX = x;
        int startY = y;
        distance = int.MaxValue;
        while (x > 0 && x < map.width - 1 && y > 0 && y < map.height - 1)
        {
            if (tiles[y][x] == TileType.FLOOR)
            {
                if (!map.tileObjects[y][x].isFloodFilled)
                {
                    switch (dir)
                    {
                        case Direction.UP: distance = y - startY; break;
                        case Direction.DOWN: distance = startY - y; break;
                        case Direction.RIGHT: distance = x - startX; break;
                        case Direction.LEFT: distance = startX - x; break;
                    }

                    return map.tileObjects[y][x];
                }
                else
                {
                    return null;
                }
            }
            switch (dir)
            {
                case Direction.UP: y++; break;
                case Direction.DOWN: y--; break;
                case Direction.RIGHT: x++; break;
                case Direction.LEFT: x--; break;
            }
        }

        return null;
    }

    void CreatePathToFloor(Map map, Direction dir, int x, int y)
    {
        int startX = x;
        int startY = y;
        while (x > 0 && x < map.width && y > 0 && y < map.height - 1)
        {
            if (tiles[y][x] == TileType.FLOOR) break;

            if (dir == Direction.UP || dir == Direction.DOWN)
            {
                if (tiles[y][x - 1] == TileType.NOTHING)
                {
                    tiles[y][x - 1] = TileType.WALL;
                }
                if (tiles[y][x + 1] == TileType.NOTHING)
                {
                    tiles[y][x + 1] = TileType.WALL;
                }
            }
            else
            {
                if (tiles[y - 1][x] == TileType.NOTHING)
                {
                    tiles[y - 1][x] = TileType.WALL;
                }
                if (tiles[y + 1][x] == TileType.NOTHING)
                {
                    tiles[y + 1][x] = TileType.WALL;
                }
            }

            tiles[y][x] = TileType.FLOOR;

            switch (dir)
            {
                case Direction.UP: y++; break;
                case Direction.DOWN: y--; break;
                case Direction.RIGHT: x++; break;
                case Direction.LEFT: x--; break;
            }
        }

        tiles[startY][startX] = TileType.DOOR;
    }

    bool CreateConnectingPath(Map map, ref int pathStartX, ref int pathStartY)
    {
        wallTiles = wallTiles.OrderBy(x => UnityEngine.Random.value).ToList();

        int minDistance = int.MaxValue;
        Direction minDirection = Direction.UP;
        Tile minTile = null;
        foreach (var wallTile in wallTiles)
        {
            int hallwayLength;
            FindFloorTile(map, Direction.UP, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.UP;
                minDistance = hallwayLength;
            }
            FindFloorTile(map, Direction.DOWN, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.DOWN;
                minDistance = hallwayLength;
            }
            FindFloorTile(map, Direction.RIGHT, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.RIGHT;
                minDistance = hallwayLength;
            }
            FindFloorTile(map, Direction.LEFT, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.LEFT;
                minDistance = hallwayLength;
            }
        }

        if (minTile == null) return false;

        pathStartX = minTile.x;
        pathStartY = minTile.y;

        CreatePathToFloor(map, minDirection, minTile.x, minTile.y);

        return true;
    }
}
