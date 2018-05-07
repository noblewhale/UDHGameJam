using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Tile[] tileSet;
    public Material polarWarpMaterial;

    public int[][] tiles;
    Tile[][] tileObjects;

    public int width = 10;
    public int height = 1;

    public float tileWidth = 1;
    public float tileHeight = 1;

    public List<Tile> walls = new List<Tile>();
    public List<Tile> floors = new List<Tile>();

    public event Action OnMapLoaded;

    enum Direction
    {
        UP, DOWN, RIGHT, LEFT
    }

	void Start ()
    {
        Camera.main.orthographicSize = (width * tileWidth / Camera.main.aspect) / 2.0f;
        transform.position = new Vector3(-width * tileWidth / 2.0f, -height * tileHeight / 2.0f);
        tileObjects = new Tile[height][];
        tiles = new int[height][];
        for (int y = 0; y < height; y++) tileObjects[y] = new Tile[width];
        for (int y = 0; y < height; y++) tiles[y] = new int[width];
        GenerateMap();
	}

    private void Update()
    {
    }

    void GenerateMap()
    {
        GenerateRooms(40);
        UpdateTiles();
        PostProcessMap();
        if (OnMapLoaded != null) OnMapLoaded();
    }

    void UpdateTiles()
    { 
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tileObjects[y][x] != null && tileObjects[y][x].value != tiles[y][x])
                {
                    Destroy(tileObjects[y][x].gameObject);
                    tileObjects[y][x] = Instantiate(tileSet[tiles[y][x]].gameObject).GetComponent<Tile>();
                }
                if (tileObjects[y][x] == null)
                {
                    tileObjects[y][x] = Instantiate(tileSet[tiles[y][x]].gameObject).GetComponent<Tile>();
                }
                Tile tile = tileObjects[y][x].GetComponent<Tile>();
                tile.Init(this, tiles[y][x], x, y);
                tileObjects[y][x] = tile;
            }
        }
    }

    void FloodFill(int floodX, int floodY)
    {
        if (floodX < 0 || floodX >= width || floodY < 0 || floodY >= height) return;
        if (tileObjects[floodY][floodX].isFloodFilled) return;

        if (tiles[floodY][floodX] == 1)
        {
            tileObjects[floodY][floodX].isFloodFilled = true;
            //tileObjects[floodY][floodX].glyph.color = Color.green;
        }
        else
        {
            if (tiles[floodY][floodX] == 2)
            {
                //tileObjects[floodY][floodX].glyph.color = Color.red;
                walls.Add(tileObjects[floodY][floodX]);
            }
            return;
        }

        FloodFill(floodX - 1, floodY);
        FloodFill(floodX + 1, floodY);
        FloodFill(floodX, floodY - 1);
        FloodFill(floodX, floodY + 1);
    }
    
    Tile FindFloorTile(Direction dir, int x, int y, out int distance)
    {
        int startX = x;
        int startY = y;
        distance = int.MaxValue;
        while (x > 0 && x < width -1 && y > 0 && y < height - 1)
        {
            if (tiles[y][x] == 1)
            {
                if (!tileObjects[y][x].isFloodFilled)
                {
                    switch (dir)
                    {
                        case Direction.UP: distance = y - startY; break;
                        case Direction.DOWN: distance = startY - y; break;
                        case Direction.RIGHT: distance = x - startX; break;
                        case Direction.LEFT: distance = startX - x; break;
                    }

                    return tileObjects[y][x];
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

    void CreatePathToFloor(Direction dir, int x, int y)
    {
        while (x > 0 && x < width - 1 && y > 0 && y < height - 1)
        {
            if (tiles[y][x] == 1) break;

            if (dir == Direction.UP || dir == Direction.DOWN)
            {
                if (tiles[y][x - 1] == 0)
                {
                    SetTile(x - 1, y, 2);
                }
                if (tiles[y][x + 1] == 0)
                {
                    SetTile(x + 1, y, 2);
                }
            }
            else
            {
                if (tiles[y-1][x] == 0)
                {
                    SetTile(x, y - 1, 2);
                }
                if (tiles[y+1][x] == 0)
                {
                    SetTile(x, y + 1, 2);
                }
            }

            SetTile(x, y, 1);

            switch (dir)
            {
                case Direction.UP: y++; break;
                case Direction.DOWN: y--; break;
                case Direction.RIGHT: x++; break;
                case Direction.LEFT: x--; break;
            }
        }
    }

    void SetTile(int x, int y, int value)
    {
        tiles[y][x] = value;
        if (tileObjects[y][x] != null || tileObjects[y][x].gameObject != null)
        {
            Destroy(tileObjects[y][x].gameObject);
        }
        tileObjects[y][x] = GameObject.Instantiate(tileSet[value]).GetComponent<Tile>();
        tileObjects[y][x].Init(this, value, x, y);
    }

    bool CreateConnectingPath(ref int pathStartX, ref int pathStartY)
    {
        walls = walls.OrderBy(x => UnityEngine.Random.value).ToList();

        int minDistance = int.MaxValue;
        Direction minDirection = Direction.UP;
        Tile minTile = null;
        foreach (var wallTile in walls)
        {
            int hallwayLength;
            Tile floorTile = FindFloorTile(Direction.UP, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.UP;
                minDistance = hallwayLength;
            }
            floorTile = FindFloorTile(Direction.DOWN, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.DOWN;
                minDistance = hallwayLength;
            }
            floorTile = FindFloorTile(Direction.RIGHT, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.RIGHT;
                minDistance = hallwayLength;
            }
            floorTile = FindFloorTile(Direction.LEFT, wallTile.x, wallTile.y, out hallwayLength);
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
        
        CreatePathToFloor(minDirection, minTile.x, minTile.y);

        return true;
    }

    void PostProcessMap()
    {
        int floodX = 0, floodY = 0;

        bool floorTileFound = false;
        do
        {
            floorTileFound = false;
            for (floodY = 1; floodY < height - 1; floodY++)
            {
                for (floodX = 1; floodX < width - 1; floodX++)
                {
                    if (tiles[floodY][floodX] == 1 && !tileObjects[floodY][floodX].isFloodFilled)
                    {
                        floorTileFound = true;
                        break;
                    }
                }
                if (floorTileFound) break;
            }

            if (floorTileFound)
            {
                Debug.Log(floodX + " " + floodY);
                bool foundPath = true;

                while (foundPath)
                {
                    FloodFill(floodX, floodY);
                    foundPath = CreateConnectingPath(ref floodX, ref floodY);
                }
            }
        } while (floorTileFound);

        walls.Clear();
        floors.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tiles[y][x] == 1) floors.Add(tileObjects[y][x]);
                if (tiles[y][x] == 2) walls.Add(tileObjects[y][x]);
            }
        }
    }

    void GenerateRooms(int numRooms)
    {
        for (int n = 0; n < numRooms; n++)
        {
            int w = UnityEngine.Random.Range(3, width / 3);
            int h = UnityEngine.Random.Range(3, height / 3);
            int x = UnityEngine.Random.Range(0, width - 3);
            int y = UnityEngine.Random.Range(0, height - 3);

            GenerateRoom(x, y, w, h);
        }
    }

    void GenerateRoom(int x, int y, int w, int h)
    {
        for (int yi = 0; yi < h; yi++)
        {
            for (int xi = 0; xi < w; xi++)
            {
                if (y + yi >= height || x + xi >= width) continue;
                
                if (yi == 0 || yi == h-1 || xi == 0 || xi == w-1 || y+yi == height -1 || x+xi == width -1)
                {
                    if (tiles[y + yi][x + xi] != 1)
                    {
                        tiles[y + yi][x + xi] = 2;
                    }
                }
                else
                {
                    tiles[y + yi][x + xi] = 1;
                }
            }
        }
    }
}
