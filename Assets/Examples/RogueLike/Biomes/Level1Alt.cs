using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Level1Alt : BiomeType
{
    public BiomeDropRate[] nothings;
    public BiomeDropRate[] floors;
    public BiomeDropRate[] walls;
    public BiomeDropRate[] doors;
    public DungeonObject finalDoorPrefab;
    public GameObject debugObject;

    public int minBSPArea = 4;

    class Node
    {
        public Node left;
        public Node right;
        public Node parent;
        public RectIntExclusive area;
    }

    Node root;

    public enum TileType
    {
        NOTHING, FLOOR, WALL, DOOR
    }

    public TileType[][] tiles;

    override public void PreProcessMap(Map map, RectIntExclusive area)
    {
        tiles = new TileType[map.height][];
        for (int y = 0; y < map.height; y++) tiles[y] = new TileType[map.width];

        root = new Node();
        root.area = area;
        GenerateRooms(root, map, 1);
        //UpdateTiles(map, area);
        //SpawnFinalDoor(map, area);
    }

    override public void DrawDebug(Map map, RectIntExclusive area)
    {
        base.DrawDebug(map, area);
        DrawArea(map, root);
    }

    void DrawArea(Map map, Node parent)
    {
        if (parent == null) return;
        EditorUtil.DrawRect(map, parent.area, Color.green);
        DrawArea(map, parent.left);
        DrawArea(map, parent.right);
    }

    void GenerateRooms(Node parent, Map map, float splitProbability)
    {
        //var debugOb = Instantiate(debugObject);
        //float x = parent.area.xMin + (parent.area.width + 1) / 2.0f;
        //x *= map.tileWidth;
        //x += map.transform.position.x;
        //float y = parent.area.yMin + (parent.area.height + 1) / 2.0f;
        //y *= map.tileHeight;
        //y += map.transform.position.y;
        //debugOb.transform.position = new Vector3(x, y, 0);
        //float width = (parent.area.width + 1) * map.tileWidth;
        //float height = (parent.area.height + 1) * map.tileHeight;
        //debugOb.transform.localScale = new Vector3(width, height, 1);
        if (Random.value > splitProbability) return;

        bool horizontalHasRoom = parent.area.width >= minBSPArea * 2;
        bool verticalHasRoom = parent.area.height >= minBSPArea * 2;
        if (!horizontalHasRoom && !verticalHasRoom) return;

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
            if (Random.value > .5f) splitHorizontal = false;
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
            GenerateRooms(child1, map, splitProbability * .9f);

            newArea = new RectIntExclusive();
            newArea.xMin = splitX + 1;
            newArea.xMax = parent.area.xMax;
            newArea.yMin = parent.area.yMin;
            newArea.yMax = parent.area.yMax;
            Node child2 = new Node();
            child2.area = newArea;
            child2.parent = parent;
            parent.right = child2;
            GenerateRooms(child2, map, splitProbability * .9f);
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
            GenerateRooms(child1, map, splitProbability * .9f);

            newArea = new RectIntExclusive();
            newArea.xMin = parent.area.xMin;
            newArea.xMax = parent.area.xMax;
            newArea.yMin = splitY + 1;
            newArea.yMax = parent.area.yMax;
            Node child2 = new Node();
            child2.area = newArea;
            child2.parent = parent;
            parent.right = child2;
            GenerateRooms(child2, map, splitProbability * .9f);
        }
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

    void AddTileObjects(Map map, int x, int y)
    {
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
