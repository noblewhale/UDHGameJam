using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Tile tilePrefab;
    public DungeonObject[] objectSet;
    public Material polarWarpMaterial;

    public int[][] tiles;
    public Tile[][] tileObjects;

    public int width = 10;
    public int height = 1;

    public float tileWidth = 1;
    public float tileHeight = 1;

    public float TotalWidth {
        get {
            return width * tileWidth;
        }
    }

    public float TotalHeight{
        get {
            return height * tileHeight;
        }
    }

    public List<Tile> walls = new List<Tile>();
    public List<Tile> floors = new List<Tile>();

    public event Action OnMapLoaded;

    public float mapGenerationAnimationDelay = 0;

	void Start ()
    {
        Camera.main.orthographicSize = (width * tileWidth / Camera.main.aspect) / 2.0f;
        transform.position = new Vector3(-width * tileWidth / 2.0f, -height * tileHeight / 2.0f);
        tileObjects = new Tile[height][];
        tiles = new int[height][];
        for (int y = 0; y < height; y++) tileObjects[y] = new Tile[width];
        for (int y = 0; y < height; y++) tiles[y] = new int[width];
        StartCoroutine(GenerateMap());
	}

    private void Update()
    {
    }

    IEnumerator GenerateMap()
    {
        yield return StartCoroutine(GenerateRooms(30));
        UpdateTiles();
        StartCoroutine(PostProcessMap());
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
                    AddTile(x, y);
                }
                if (tileObjects[y][x] == null)
                {
                    AddTile(x, y);
                }
                Tile tile = tileObjects[y][x].GetComponent<Tile>();
                tile.Init(this, tiles[y][x], x, y);
                tileObjects[y][x] = tile;
            }
        }
    }

    void AddTile(int x, int y)
    {
        tileObjects[y][x] = Instantiate(tilePrefab, new Vector3(-666, -666, -666), Quaternion.identity).GetComponent<Tile>();
        tileObjects[y][x].Init(this, tiles[y][x], x, y);
        if (tiles[y][x] != 0)
        {
            tileObjects[y][x].SpawnAndAddObject(objectSet[1]);
            if (tiles[y][x] != 1)
            {
                tileObjects[y][x].SpawnAndAddObject(objectSet[tiles[y][x]]);
            }
        }
    }

    void FloodFill(int floodX, int floodY)
    {
        if (floodY < 0 || floodY >= height) return;

        int wrappedX = floodX;
        if (wrappedX < 0) wrappedX = width + wrappedX;
        else if (wrappedX >= width) wrappedX = wrappedX - width;

        if (tileObjects[floodY][wrappedX].isFloodFilled) return;

        if (tiles[floodY][wrappedX] != 2)
        {
            tileObjects[floodY][wrappedX].isFloodFilled = true;
            //tileObjects[floodY][wrappedX].glyph.color = Color.green;
        }
        else
        {
            if (tiles[floodY][wrappedX] == 2)
            {
                //tileObjects[floodY][wrappedX].glyph.color = Color.red;
                walls.Add(tileObjects[floodY][wrappedX]);
            }
            return;
        }

        FloodFill(wrappedX - 1, floodY);
        FloodFill(wrappedX + 1, floodY);
        FloodFill(wrappedX, floodY - 1);
        FloodFill(wrappedX, floodY + 1);
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

    public float WrapX(float x)
    {
        if (x < 0) x += width;
        else if (x >= width) x -= width;

        return x;
    }

    public int WrapX(int x)
    {
        if (x < 0) x += width;
        else if (x >= width) x -= width;

        return x;
    }

    public void ForEachTile(Action<Tile> action)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                action(tileObjects[y][x]);
            }
        }
    }

    internal void Reveal(int tileX, int tileY, float radius)
    {
        ForEachTile(t => t.isInView = false);
        tileObjects[tileY][tileX].SetRevealed(true);
        Vector2 center = new Vector2(tileX + .5f, tileY + .5f);
        int numRays = 360;
        float stepSize = .33f;
        for (int r = 0; r < numRays; r++)
        {
            float dirX = Mathf.Sin(2 * Mathf.PI * r / numRays);
            float dirY = Mathf.Cos(2 * Mathf.PI * r / numRays);
            Vector2 direction = new Vector2(dirX, dirY);

            for (int d = 1; d < radius / stepSize; d++)
            {
                Vector2 relative = center + direction * d * stepSize;

                int y = (int)relative.y;
                if (y < 0 || y >= height) break;

                int wrappedX = (int)WrapX(relative.x);

                tileObjects[y][wrappedX].SetRevealed(true);
                if (tileObjects[y][wrappedX].DoesBlockLineOfSight())
                {
                    break;
                }
            }
        }
    }

    void CreatePathToFloor(Direction dir, int x, int y)
    {
        int startX = x;
        int startY = y;
        while (x > 0 && x < width && y > 0 && y < height - 1)
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

        SetTile(startX, startY, 3);
        //tileObjects[startY][startX].isFloodFilled = true;
    }

    void SetTile(int x, int y, int value)
    {
        tiles[y][x] = value;
        if (tileObjects[y][x] != null || tileObjects[y][x].gameObject != null)
        {
            Destroy(tileObjects[y][x].gameObject);
        }
        AddTile(x, y);
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
            FindFloorTile(Direction.UP, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.UP;
                minDistance = hallwayLength;
            }
            FindFloorTile(Direction.DOWN, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.DOWN;
                minDistance = hallwayLength;
            }
            FindFloorTile(Direction.RIGHT, wallTile.x, wallTile.y, out hallwayLength);
            if (hallwayLength < minDistance)
            {
                minTile = wallTile;
                minDirection = Direction.RIGHT;
                minDistance = hallwayLength;
            }
            FindFloorTile(Direction.LEFT, wallTile.x, wallTile.y, out hallwayLength);
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

    IEnumerator PostProcessMap()
    {
        int floodX = 0, floodY = 0;
        if (mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(mapGenerationAnimationDelay);
        bool floorTileFound = false;
        do
        {
            floorTileFound = false;
            for (floodY = 1; floodY < height - 1; floodY++)
            {
                for (floodX = 0; floodX < width; floodX++)
                {
                    if (tiles[floodY][floodX] == 1 && !tileObjects[floodY][floodX].isFloodFilled)
                    {
                        floorTileFound = true;
                        break;
                    }
                }
                if (floorTileFound) break;
            }

            if (mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(mapGenerationAnimationDelay);

            if (floorTileFound)
            {
                Debug.Log(floodX + " " + floodY);
                bool foundPath = true;

                while (foundPath)
                {
                    FloodFill(floodX, floodY);

                    if (mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(mapGenerationAnimationDelay);

                    foundPath = CreateConnectingPath(ref floodX, ref floodY);

                    if (mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(mapGenerationAnimationDelay);
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
        
        if (OnMapLoaded != null) OnMapLoaded();
    }

    IEnumerator GenerateRooms(int numRooms)
    {
        if (mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(mapGenerationAnimationDelay * 2);
        for (int n = 0; n < numRooms; n++)
        {
            int w = UnityEngine.Random.Range(3, width / 3);
            int h = UnityEngine.Random.Range(3, height / 3);
            int x = UnityEngine.Random.Range(0, width - 1);
            int y = UnityEngine.Random.Range(0, height - 3);

            GenerateRoom(x, y, w, h);
            UpdateTiles();

            if (mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(mapGenerationAnimationDelay / 10);
        }
    }

    void GenerateRoom(int x, int y, int w, int h)
    {
        for (int yi = 0; yi < h; yi++)
        {
            for (int xi = 0; xi < w; xi++)
            {
                if (y + yi >= height) continue;

                int wrappedX = x + xi;
                if (wrappedX >= width) wrappedX = wrappedX - width;
                
                if (yi == 0 || yi == h-1 || xi == 0 || xi == w-1 || y+yi == height -1)
                {
                    if (tiles[y + yi][wrappedX] != 1)
                    {
                        tiles[y + yi][wrappedX] = 2;
                    }
                }
                else
                {
                    tiles[y + yi][wrappedX] = 1;
                }
            }
        }
    }
}
