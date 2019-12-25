using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{

    public Tile tilePrefab;
    public DungeonObject[] objectSet;

    public BiomeType[] biomeTypes;
    public List<Biome> biomes = new List<Biome>();

    public Tile[][] tileObjects;
    public List<Tile> tilesThatAllowSpawn = new List<Tile>();

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

    [NonSerialized]
    public List<Tile> tilesInRandomOrder = new List<Tile>();

    public event Action OnMapLoaded;

    public float mapGenerationAnimationDelay = 0;

    public int dungeonLevel = 1;

	void Start ()
    {
        Camera.main.orthographicSize = (width * tileWidth / Camera.main.aspect) / 2.0f;
        transform.position = new Vector3(-width * tileWidth / 2.0f, -height * tileHeight / 2.0f);

        ClearMap();
        GenerateMap();
	}

    public void ClearMap()
    {
        //if (CreatureSpawner.instance)
        //{
        //    CreatureSpawner.instance.KillAll();
        //}
        ForEachTile(t =>
        {
            t.DestroyAllObjects();
            Destroy(t.gameObject);
        });
        tileObjects = new Tile[height][];
        for (int y = 0; y < height; y++) tileObjects[y] = new Tile[width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                AddTile(x, y);
            }
        }
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
        GenerateMap();
        Player.instance.ResetInput();
    }

    void GenerateMap()
    {
        PlaceBiomes();
        PreProcessBiomes();
        ForEachTileThatAllowsSpawn(Biome.SpawnRandomObject);
        PostProcessMap();
        if (OnMapLoaded != null) OnMapLoaded();
    }

    void PlaceBiomes()
    {
        foreach (var biomeTypeTemplate in biomeTypes)
        {
            Biome biome = new Biome();
            var biomeType = Instantiate(biomeTypeTemplate);
            biome.biomeType = biomeType;
            if (biomeType.minWidth == -1) biomeType.minWidth = width;
            if (biomeType.maxWidth == -1) biomeType.maxWidth = width;
            if (biomeType.minHeight == -1) biomeType.minHeight = height;
            if (biomeType.maxHeight == -1) biomeType.maxHeight = height;
            if (biomeType.minX == -1) biomeType.minX = width - 1;
            if (biomeType.maxX == -1) biomeType.maxX = width - 1;
            if (biomeType.minY == -1) biomeType.minY = height - 1;
            if (biomeType.maxY == -1) biomeType.maxY = height - 1;

            int biomeWidth = Random.Range(biomeType.minWidth, biomeType.maxWidth);
            int biomeHeight = Random.Range(biomeType.minHeight, biomeType.maxHeight);
            int biomeX = Random.Range(biomeType.minX, biomeType.maxX - biomeWidth);
            int biomeY = Random.Range(biomeType.minY, biomeType.maxY - biomeHeight);
            biome.area = new RectInt(biomeX, biomeY, biomeWidth, biomeHeight);
            Debug.Log(biome.area);
            biomes.Add(biome);
        }
    }
    
    void PreProcessBiomes()
    {
        foreach (var biome in biomes)
        {
            biome.biomeType.PreProcessMap(this, biome.area);
        }
    }

    void PostProcessMap()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tilesInRandomOrder.Add(tileObjects[y][x]);
            }
        }

        tilesInRandomOrder = tilesInRandomOrder.OrderBy(a => UnityEngine.Random.value).ToList();
        //PlaceFinalDoor();
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

    public void AddTile(int x, int y)
    {
        tileObjects[y][x] = Instantiate(tilePrefab, new Vector3(-666, -666, -666), Quaternion.identity).GetComponent<Tile>();
        tileObjects[y][x].Init(this, x, y);
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

    internal void MoveObject(DungeonObject ob, int newX, int newY)
    {
        tileObjects[ob.y][ob.x].RemoveObject(ob);
        tileObjects[newY][newX].AddObject(ob);
    }

    public void Reveal(int tileX, int tileY, float radius)
    {
        ForEachTile(t => t.SetInView(false));
        tileObjects[tileY][tileX].SetInView(true);
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

                tileObjects[y][wrappedX].SetInView(true);
                if (tileObjects[y][wrappedX].DoesBlockLineOfSight())
                {
                    break;
                }
            }
        }
    }

    public void ForEachTileThatAllowsSpawn(Action<Tile> doThis, RectInt area)
    {
        // If the area is larger than the total number of floor tiles it is more efficient to use the precomputed list
        if (area.width * area.height > tilesThatAllowSpawn.Count)
        {
            ForEachTileThatAllowsSpawn(doThis);
            return;
        }
        for (int y = area.yMax-1; y >= area.yMin; y--)
        {
            for (int x = area.xMax-1; x >= area.xMin; x--)
            {
                var tile = tileObjects[y][x];
                if (tile.AllowsSpawn())
                {
                    doThis(tile);
                }
            }
        }
    }

    public void ForEachTileThatAllowsSpawn(Action<Tile> doThis)
    {
        for (int i = tilesThatAllowSpawn.Count-1; i >= 0; i--)
        {
            var tile = tilesThatAllowSpawn[i];
            doThis(tile);
        }
    }

    public Tile GetRandomTileThatAllowsSpawn()
    {
        return tilesThatAllowSpawn[UnityEngine.Random.Range(0, tilesThatAllowSpawn.Count)];
    }
}
