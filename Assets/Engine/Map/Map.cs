namespace Noble.TileEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class Map : MonoBehaviour
    {

        public Tile tilePrefab;

        public Biome[] biomeTypes;
        public List<Biome> biomes = new List<Biome>();

        public Tile[][] tileObjects;
        public List<Tile> tilesThatAllowSpawn = new List<Tile>();

        public DungeonObject outlinePrefab;
        public List<DungeonObject> outlineObjects = new List<DungeonObject>();

        public int width = 10;
        public int height = 1;

        public float tileWidth = 1;
        public float tileHeight = 1;

        public float TotalWidth
        {
            get
            {
                return width * tileWidth;
            }
        }

        public float TotalHeight
        {
            get
            {
                return height * tileHeight;
            }
        }

        [NonSerialized]
        public List<Tile> tilesInRandomOrder = new List<Tile>();

        public event Action OnMapLoaded;
        public event Action OnMapGenerationStarted;
        public event Action OnMapCleared;

        public float mapGenerationAnimationDelay = 0;

        public static Map instance;

        public Camera warpedMapCamera;

        public bool isDoneGeneratingMap = false;

        virtual public void Awake()
        {
            instance = this;
            transform.position = new Vector3(-TotalWidth / 2.0f, -TotalHeight / 2.0f);
        }

        virtual public void Start()
        {
            ClearMap();
            StartCoroutine(GenerateMap());
        }

        virtual public void ClearMap()
        {
            isDoneGeneratingMap = false;
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

            foreach (var biome in biomes)
            {
                Destroy(biome);
            }
            biomes.Clear();

            OnMapCleared?.Invoke();
        }

        virtual public IEnumerator RegenerateMap()
        {
            ClearMap();
            yield return StartCoroutine(GenerateMap());
        }

        virtual public IEnumerator GenerateMap()
        {
            isDoneGeneratingMap = false;

            OnMapGenerationStarted?.Invoke();

            //UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));

            // Place the biomes
            PlaceBiomes();

            // Pre-process each biome
            yield return StartCoroutine(PreProcessBiomes());

            // Spawn things on tiles based on biomes
            ForEachTile(Biome.SpawnRandomObject);

            isDoneGeneratingMap = true;

            OnMapLoaded?.Invoke();
        }

        virtual public void Update()
        {
            foreach (var biome in biomes)
            {
                biome.DrawDebug();
            }

            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    ClearMap();
            //    StartCoroutine(GenerateMap());
            //}
        }

        virtual public void PlaceBiomes()
        {
            foreach (var biomeType in biomeTypes)
            {
                if (biomeType == null) continue;

                // Make a copy of the boime type so we don't modify properties on the actual asset
                var biome = Instantiate(biomeType);
                if (biome.minWidth == -1) biome.minWidth = width;
                if (biome.maxWidth == -1) biome.maxWidth = width;
                if (biome.minHeight == -1) biome.minHeight = height;
                if (biome.maxHeight == -1) biome.maxHeight = height;
                if (biome.minX == -1) biome.minX = width - 1;
                if (biome.maxX == -1) biome.maxX = width;
                if (biome.minY == -1) biome.minY = height - 1;
                if (biome.maxY == -1) biome.maxY = height;

                int biomeWidth = Random.Range(biome.minWidth, biome.maxWidth);
                int biomeHeight = Random.Range(biome.minHeight, biome.maxHeight);
                int biomeX = Random.Range(biome.minX, biome.maxX - biomeWidth + 1);
                int biomeY = Random.Range(biome.minY, biome.maxY - biomeHeight + 1);
                biome.area = new RectIntExclusive(biomeX, biomeY, biomeWidth, biomeHeight);
                biomes.Add(biome);
            }
        }

        virtual public IEnumerator PreProcessBiomes()
        {
            foreach (var biome in biomes)
            {
                yield return StartCoroutine(biome.PreProcessMap(this));
            }
        }

        virtual public void AddTile(int x, int y)
        {
            tileObjects[y][x] = Instantiate(tilePrefab, new Vector3(-666, -666, -666), Quaternion.identity).GetComponent<Tile>();
            tileObjects[y][x].Init(this, x, y);
        }

        virtual public Vector2 GetPositionOnMap(Vector2 pos)
        {
            pos.x = GetXPositionOnMap(pos.x);
            pos.y = GetYPositionOnMap(pos.y);

            return pos;
        }

        virtual public Vector2Int GetPositionOnMap(Vector2Int pos)
        {
            pos.x = GetXPositionOnMap(pos.x);
            pos.y = GetYPositionOnMap(pos.y);

            return pos;
        }

        virtual public int GetXPositionOnMap(int x)
        {
            return Math.Clamp(x, 0, width - 1);
        }

        virtual public int GetYPositionOnMap(int y)
        {
            return Math.Clamp(y, 0, height - 1);
        }

        virtual public float GetXPositionOnMap(float x)
        {
            return Mathf.Clamp(x, 0, TotalWidth);
        }

        virtual public float GetYPositionOnMap(float y)
        {
            return Mathf.Clamp(y, 0, TotalHeight);
        }

        virtual public Tile GetTile(int x, int y) => GetTile(new Vector2Int(x, y));
        virtual public Tile GetTile(float x, float y) => GetTile(new Vector2(x, y));

        virtual public Tile GetTile(Vector2Int position)
        {
            position = GetPositionOnMap(position);
            return tileObjects[position.y][position.x];
        }

        virtual public Tile GetTile(Vector2 position)
        {
            position = GetPositionOnMap(position);
            return tileObjects[(int)position.y][(int)position.x];
        }

        virtual public void ForEachTile(Action<Tile> action)
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

        virtual public void ForEachTileInArea(RectIntExclusive area, Action<Tile> action)
        {
            for (int y = Math.Min(area.yMax - 1, height - 1); y >= Math.Max(area.yMin, 0); y--)
            {
                for (int x = area.xMax - 1; x >= area.xMin; x--)
                {
                    var tile = GetTile(x, y);
                    action(tile);
                }
            }
        }

        virtual public void ForEachTileInPerimeter(RectIntExclusive area, Action<Tile> action)
        {
            Tile tile;
            for (int x = area.xMax - 1; x >= area.xMin; x--)
            {
                tile = GetTile(x, Math.Min(area.yMax - 1, height - 1));
                action(tile);
            }
            for (int y = Math.Min(area.yMax - 1, height - 1); y >= Math.Max(area.yMin, 0); y--)
            {
                tile = GetTile(area.xMin, y);
                action(tile);

                tile = GetTile(area.xMax - 1, y);
                action(tile);
            }
            for (int x = area.xMax - 1; x >= area.xMin; x--)
            {
                tile = GetTile(x, Math.Max(area.yMin, 0));
                action(tile);
            }
        }

        virtual public List<Tile> GetTilesOfType(string type, RectIntExclusive area)
        {
            var tiles = new List<Tile>();

            ForEachTileInArea(area, (tile) =>
            {
                if (tile.ContainsObjectOfType(type))
                {
                    tiles.Add(tile);
                }
            });

            return tiles;
        }

        virtual public void TryMoveObject(DungeonObject ob, int newX, int newY)
        {
            if (!tileObjects[newY][newX].IsCollidable())
            {
                MoveObject(ob, newX, newY);
            }
            else
            {
                Tile collidedTile = tileObjects[newY][newX];
                foreach (var tileObject in collidedTile.objectList)
                {
                    if (tileObject.isCollidable)
                    {
                        ob.CollideWith(tileObject, true);
                        tileObject.CollideWith(ob, false);
                        break;
                    }
                }
            }
        }

        virtual public void MoveObject(DungeonObject ob, int newX, int newY)
        {
            tileObjects[ob.y][ob.x].RemoveObject(ob);
            tileObjects[newY][newX].AddObject(ob, true);
        }

        virtual public void AddOutline(int tileX, int tileY, int radius)
        {
            ForEachTileInRadius(
                tileX, tileY, 
                radius,
                (Tile t) =>
                {
                    outlineObjects.Add(t.SpawnAndAddObject(outlinePrefab, 1, true));
                }
            );
        }

        virtual public void RemoveOutline()
        {
            foreach (DungeonObject outlineObject in outlineObjects)
            {
                outlineObject.tile.RemoveObject(outlineObject, true);
            }
            outlineObjects.Clear();
        }

        virtual public void Reveal(int tileX, int tileY, float radius)
        {
            tileObjects[tileY][tileX].SetInView(true);

            ForEachTileInRadius(
                tileX, tileY, 
                radius, 
                (Tile t) =>
                {
                    tileObjects[t.y][t.x].SetInView(true);
                },
                (Tile t) => 
                {
                    return tileObjects[t.y][t.x].DoesBlockLineOfSight() && (t.y != tileY || t.x != tileX);
                }
            );
        }

        virtual public List<Tile> GetTilesInRadius(int centerX, int centerY, float radius)
        {
            List<Tile> tiles = new List<Tile>();
            ForEachTileInRadius(centerX, centerY, radius, t => tiles.Add(t));
            return tiles;
        }

        virtual public void ForEachTileInRadius(int centerX, int centerY, float radius, Action<Tile> action, Func<Tile, bool> stopCondition = null)
        {
            // Start from the center of the tile
            Vector2 center = new Vector2(centerX + .5f, centerY + .5f);

            var area = new RectIntExclusive(
                (int)(centerX - radius - 1),
                (int)(centerY - radius - 1),
                (int)(radius * 2 + 3),
                (int)(radius * 2 + 3)
            );
            ForEachTileInArea(area, (t) => t.isDirty = false);

            for (float r = 0; r < Mathf.PI * 2; r += 2 * Mathf.PI / 360)
            {
                Vector2 direction = new Vector2(Mathf.Sin(r), Mathf.Cos(r));

                //Debug.DrawLine((Vector2)transform.position + center, (Vector2)transform.position + center + direction * radius, Color.magenta, 4);
                ForEachTileInRay(center, direction, radius, action, stopCondition);
            }
        }

        virtual public Vector2 GetDifference(Vector2 start, Vector2 end) => new Vector2(GetXDifference(start.x, end.x), GetYDifference(start.y, end.y));

        virtual public Vector2Int GetDifference(Vector2Int start, Vector2Int end) => new Vector2Int(GetXDifference(start.x, end.x), GetYDifference(start.y, end.y));

        virtual public float GetXDifference(float startX, float endX) => endX - startX;

        virtual public float GetYDifference(float startY, float endY) => endY - startY;

        virtual public int GetXDifference(int startX, int endX) => endX - startX;

        virtual public int GetYDifference(int startY, int endY) => endY - startY;

        virtual public void ForEachTileInRay(Vector2 start, Vector2 dir, float distance, Action<Tile> action, Func<Tile, bool> stopCondition = null)
        {
            float stepSize = .4f;

            for (int d = 1; d < distance / stepSize; d++)
            {
                Vector2 relative = start + dir * d * stepSize;

                int y = (int)relative.y;
                if (y < 0 || y >= height) break;

                var tile = GetTile(relative.x, y);

                if (!tile.isDirty)
                {
                    tile.isDirty = true;
                    action(tile);
                }

                if (stopCondition != null && stopCondition(tile)) break;
            }
        }

        virtual public void UpdateLighting()
        {
            ForEachTile(t => t.SetLit(false));
            ForEachTile(t => t.UpdateLighting());
        }

        virtual public void UpdateLighting(RectIntExclusive area)
        {
            ForEachTileInArea(area, t => t.SetLit(false));
            ForEachTileInArea(area, t => t.UpdateLighting());
        }

        virtual public void UpdateLighting(int x, int y, float radius)
        {
            var area = new RectIntExclusive(
                (int)(x - radius - 1), 
                (int)(y - radius - 1), 
                (int)(x + radius + 3), 
                (int)(y + radius + 3)
            );
            UpdateLighting(area);
        }

        virtual public void ForEachTileThatAllowsSpawn(Action<Tile> doThis, RectIntExclusive area)
        {
            // If the area is larger than the total number of floor tiles it is more efficient to use the precomputed list
            if (area.width * area.height > tilesThatAllowSpawn.Count)
            {
                ForEachTileThatAllowsSpawn(doThis);
                return;
            }
            for (int y = area.yMax - 1; y >= area.yMin; y--)
            {
                for (int x = area.xMax - 1; x >= area.xMin; x--)
                {
                    var tile = tileObjects[y][x];
                    if (tile.AllowsSpawn())
                    {
                        doThis(tile);
                    }
                }
            }
        }

        virtual public void ForEachTileThatAllowsSpawn(Action<Tile> doThis)
        {
            for (int i = tilesThatAllowSpawn.Count - 1; i >= 0; i--)
            {
                var tile = tilesThatAllowSpawn[i];
                doThis(tile);
            }
        }

        virtual public Tile GetRandomTileThatAllowsSpawn()
        {
            return tilesThatAllowSpawn[UnityEngine.Random.Range(0, tilesThatAllowSpawn.Count)];
        }
    }
}