using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Level1 : BiomeType
{
    override public IEnumerator PreProcessMap(Map map)
    {
        yield return map.StartCoroutine(GenerateRooms(map, 30));
        yield return map.StartCoroutine(ConnectRooms(map));
    }

    IEnumerator GenerateRooms(Map map, int numRooms)
    {
        if (map.mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(map.mapGenerationAnimationDelay * 2);
        for (int n = 0; n < numRooms; n++)
        {
            int w = Random.Range(3, map.width / 3);
            int h = Random.Range(3, map.height / 3);
            int x = Random.Range(0, map.width);
            int y = Random.Range(0, map.height - 2);

            GenerateRoom(map, x, y, w, h);
            map.UpdateTiles();

            if (map.mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(map.mapGenerationAnimationDelay / 10);
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
                    if (map.tiles[y + yi][wrappedX] != Map.TileType.FLOOR)
                    {
                        map.tiles[y + yi][wrappedX] = Map.TileType.WALL;
                    }
                }
                else
                {
                    map.tiles[y + yi][wrappedX] = Map.TileType.FLOOR;
                }
            }
        }
    }

    IEnumerator ConnectRooms(Map map)
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
                    if (map.tiles[floodY][floodX] == Map.TileType.FLOOR && !map.tileObjects[floodY][floodX].isFloodFilled)
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

                    if (map.mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(map.mapGenerationAnimationDelay);

                    foundPath = CreateConnectingPath(map, ref floodX, ref floodY);

                    if (map.mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(map.mapGenerationAnimationDelay);
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

        if (map.tiles[floodY][wrappedX] != Map.TileType.WALL)
        {
            map.tileObjects[floodY][wrappedX].isFloodFilled = true;
            //tileObjects[floodY][wrappedX].glyph.color = Color.green;
        }
        else
        {
            if (map.tiles[floodY][wrappedX] == Map.TileType.WALL)
            {
                //tileObjects[floodY][wrappedX].glyph.color = Color.red;
                map.walls.Add(map.tileObjects[floodY][wrappedX]);
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
            if (map.tiles[y][x] == Map.TileType.FLOOR)
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
            if (map.tiles[y][x] == Map.TileType.FLOOR) break;

            if (dir == Direction.UP || dir == Direction.DOWN)
            {
                if (map.tiles[y][x - 1] == 0)
                {
                    map.SetTile(x - 1, y, Map.TileType.WALL);
                }
                if (map.tiles[y][x + 1] == 0)
                {
                    map.SetTile(x + 1, y, Map.TileType.WALL);
                }
            }
            else
            {
                if (map.tiles[y - 1][x] == 0)
                {
                    map.SetTile(x, y - 1, Map.TileType.WALL);
                }
                if (map.tiles[y + 1][x] == 0)
                {
                    map.SetTile(x, y + 1, Map.TileType.WALL);
                }
            }

            map.SetTile(x, y, Map.TileType.FLOOR);

            switch (dir)
            {
                case Direction.UP: y++; break;
                case Direction.DOWN: y--; break;
                case Direction.RIGHT: x++; break;
                case Direction.LEFT: x--; break;
            }
        }

        map.SetTile(startX, startY, Map.TileType.DOOR);
    }

    bool CreateConnectingPath(Map map, ref int pathStartX, ref int pathStartY)
    {
        map.walls = map.walls.OrderBy(x => UnityEngine.Random.value).ToList();

        int minDistance = int.MaxValue;
        Direction minDirection = Direction.UP;
        Tile minTile = null;
        foreach (var wallTile in map.walls)
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
