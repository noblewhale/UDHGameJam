using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public enum TileType
    { 
        NOTHING, FLOOR, WALL, DOOR
    }

    public Tile tilePrefab;
    public DungeonObject[] objectSet;
    public Material polarWarpMaterial;

    public BiomeType[] biomeTypes;
    public List<Biome> biomes = new List<Biome>();

    public TileType[][] tiles;
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

    List<Tile> tilesInRandomOrder = new List<Tile>();
    List<Tile> walls = new List<Tile>();
    List<Tile> floors = new List<Tile>();

    public event Action OnMapLoaded;

    public float mapGenerationAnimationDelay = 0;

    public int dungeonLevel = 1;

	void Start ()
    {
        Camera.main.orthographicSize = (width * tileWidth / Camera.main.aspect) / 2.0f;
        transform.position = new Vector3(-width * tileWidth / 2.0f, -height * tileHeight / 2.0f);

        ClearMap();
        StartCoroutine(GenerateMap());
	}

    public void ClearMap()
    {
        if (CreatureSpawner.instance)
        {
            CreatureSpawner.instance.KillAll();
        }
        ForEachTile(t => {
            t.DestroyAllObjects();
            Destroy(t.gameObject);
        });
        floors.Clear();
        walls.Clear();
        tileObjects = new Tile[height][];
        tiles = new TileType[height][];
        for (int y = 0; y < height; y++) tileObjects[y] = new Tile[width];
        for (int y = 0; y < height; y++) tiles[y] = new TileType[width];
    }

    public IEnumerator RegenerateMap()
    {
        Player.instance.isInputEnabled = false;
        PlayerCamera cam = Player.instance.mainCamera.GetComponent<PlayerCamera>();
        float playerPosY = Player.instance.identity.transform.position.y + cam.cameraOffset;
        while (Mathf.Abs(cam.transform.position.y - playerPosY) > .01f)
        {
            yield return new WaitForEndOfFrame();
        }
        dungeonLevel++;
        ClearMap();
        yield return StartCoroutine(GenerateMap());
        Player.instance.ResetInput();
    }

    IEnumerator GenerateMap()
    {
        PlaceBiomes();

        yield return StartCoroutine(GenerateRooms(30));
        UpdateTiles();
        yield return StartCoroutine(PostProcessMap());
    }

    //void PlaceFinalDoor()
    //{
    //    var topTilesWithWalls = tileObjects[height - 1].Where(x => x.ContainsObjectOfType(objectSet[2]));
    //    var validFinalDoorSpots = new List<Tile>();
    //    foreach (var t in topTilesWithWalls)
    //    {
    //        var tileBeneathWall = tileObjects[height - 2][t.x];
    //        if (!tileBeneathWall.IsCollidable())
    //        {
    //            validFinalDoorSpots.Add(t);
    //        }
    //    }

    //    var tile = validFinalDoorSpots[UnityEngine.Random.Range(0, validFinalDoorSpots.Count)];
    //    var node = tile.objectList.First;
    //    while (node != null)
    //    {
    //        var nextNode = node.Next;
    //        if (node.Value.objectName == "Wall") tile.objectList.Remove(node);
    //        node = nextNode;
    //    }
    //    var lockedDoor = Instantiate(objectSet[3].gameObject).GetComponent<DungeonObject>();
    //    lockedDoor.GetComponent<Door>().SetLocked(true);
    //    tile.SpawnAndAddObject(objectSet[5]);
    //    tile.AddObject(lockedDoor);
    //}

    //void PlaceKey()
    //{
    //    var tile = floors[UnityEngine.Random.Range(0, floors.Count)];
    //    tile.SpawnAndAddObject(objectSet[4]);
    //}

    void UpdateTiles()
    { 
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tileObjects[y][x] != null && tileObjects[y][x].baseType != tiles[y][x])
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
        if (tiles[y][x] != TileType.NOTHING)
        {
            // Pick a floor tile based on biome
            DungeonObject ob = Biome.GetRandomBaseTile(x, y, biomes, TileType.FLOOR);
            tileObjects[y][x].SpawnAndAddObject(ob);
            if (tiles[y][x] != TileType.FLOOR)
            {
                var type = tiles[y][x];
                ob = Biome.GetRandomBaseTile(x, y, biomes, type);
                tileObjects[y][x].SpawnAndAddObject(ob);
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

        if (tiles[floodY][wrappedX] != TileType.WALL)
        {
            tileObjects[floodY][wrappedX].isFloodFilled = true;
            //tileObjects[floodY][wrappedX].glyph.color = Color.green;
        }
        else
        {
            if (tiles[floodY][wrappedX] == TileType.WALL)
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
            if (tiles[y][x] == TileType.FLOOR)
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
        if (tileObjects == null) return;
        if (tileObjects.Length != height) return;
        for (int y = 0; y < height; y++)
        {
            if (tileObjects[y].Length != width) return;
            for (int x = 0; x < width; x++)
            {
                action(tileObjects[y][x]);
            }
        }
    }

    public void Reveal(int tileX, int tileY, float radius)
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
            if (tiles[y][x] == TileType.FLOOR) break;

            if (dir == Direction.UP || dir == Direction.DOWN)
            {
                if (tiles[y][x - 1] == 0)
                {
                    SetTile(x - 1, y, TileType.WALL);
                }
                if (tiles[y][x + 1] == 0)
                {
                    SetTile(x + 1, y, TileType.WALL);
                }
            }
            else
            {
                if (tiles[y-1][x] == 0)
                {
                    SetTile(x, y - 1, TileType.WALL);
                }
                if (tiles[y+1][x] == 0)
                {
                    SetTile(x, y + 1, TileType.WALL);
                }
            }

            SetTile(x, y, TileType.FLOOR);

            switch (dir)
            {
                case Direction.UP: y++; break;
                case Direction.DOWN: y--; break;
                case Direction.RIGHT: x++; break;
                case Direction.LEFT: x--; break;
            }
        }

        SetTile(startX, startY, TileType.DOOR);
        //tileObjects[startY][startX].isFloodFilled = true;
    }

    void SetTile(int x, int y, TileType baseType)
    {
        tiles[y][x] = baseType;
        if (tileObjects[y][x] != null || tileObjects[y][x].gameObject != null)
        {
            Destroy(tileObjects[y][x].gameObject);
        }
        AddTile(x, y);
        tileObjects[y][x].Init(this, baseType, x, y);
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
                    if (tiles[floodY][floodX] == TileType.FLOOR && !tileObjects[floodY][floodX].isFloodFilled)
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
                if (tiles[y][x] == TileType.FLOOR) floors.Add(tileObjects[y][x]);
                if (tiles[y][x] == TileType.WALL) walls.Add(tileObjects[y][x]);
                tilesInRandomOrder.Add(tileObjects[y][x]);
            }
        }

        floors = floors.OrderBy(a => UnityEngine.Random.value).ToList();
        walls = walls.OrderBy(a => UnityEngine.Random.value).ToList();
        tilesInRandomOrder = tilesInRandomOrder.OrderBy(a => UnityEngine.Random.value).ToList();

        //PlaceKey();
        //PlaceFinalDoor();

        if (OnMapLoaded != null) OnMapLoaded();
    }

    void PlaceBiomes()
    {
        Biome biome = new Biome();
        biome.biomeType = biomeTypes[0];
        biome.area = new RectInt(0, 0, width, height);

        biomes.Add(biome);

        biome = new Biome();
        biome.biomeType = biomeTypes[1];
        int w = UnityEngine.Random.Range(3, 5);
        int h = UnityEngine.Random.Range(3, 5);
        int x = UnityEngine.Random.Range(0, width - w - 1);
        int y = UnityEngine.Random.Range(0, height - h - 1);
        biome.area = new RectInt(x, y, w, h);

        biomes.Add(biome);
    }

    IEnumerator GenerateRooms(int numRooms)
    {
        if (mapGenerationAnimationDelay != 0) yield return new WaitForSeconds(mapGenerationAnimationDelay * 2);
        for (int n = 0; n < numRooms; n++)
        {
            int w = UnityEngine.Random.Range(3, width / 3);
            int h = UnityEngine.Random.Range(3, height / 3);
            int x = UnityEngine.Random.Range(0, width);
            int y = UnityEngine.Random.Range(0, height - 2);

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
                    if (tiles[y + yi][wrappedX] != TileType.FLOOR)
                    {
                        tiles[y + yi][wrappedX] = TileType.WALL;
                    }
                }
                else
                {
                    tiles[y + yi][wrappedX] = TileType.FLOOR;
                }
            }
        }
    }

    public void ForEachFloorTile(Action<Tile> doThis)
    {
        foreach (var tile in floors) doThis(tile);
    }

    public Tile GetRandomFloorTile()
    {
        return floors[UnityEngine.Random.Range(0, floors.Count)];
    }
}
