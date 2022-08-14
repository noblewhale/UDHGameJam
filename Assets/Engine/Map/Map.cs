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
        }

        virtual public void Start()
        {
            // Set the camera size so that the width is 3 times the map width so wrapping magic works.
            warpedMapCamera.orthographicSize = (3 * TotalWidth / warpedMapCamera.aspect) / 2.0f;
            transform.position = new Vector3(-TotalWidth / 2.0f, -TotalHeight / 2.0f);

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

            if (Input.GetKeyDown(KeyCode.R))
            {
                ClearMap();
                StartCoroutine(GenerateMap());
            }
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
                if (biome.maxX == -1) biome.maxX = width - 1;
                if (biome.minY == -1) biome.minY = height - 1;
                if (biome.maxY == -1) biome.maxY = height - 1;

                int biomeWidth = Random.Range(biome.minWidth, biome.maxWidth + 1);
                int biomeHeight = Random.Range(biome.minHeight, biome.maxHeight + 1);
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

        public void ForEachTile(RectIntExclusive area, Action<Tile> action)
        {
            for (int y = area.yMax; y >= area.yMin; y--)
            {
                for (int x = area.xMax; x >= area.xMin; x--)
                {
                    var tile = tileObjects[y][x];
                    action(tile);
                }
            }
        }

        public List<Tile> GetTilesOfType(string type, RectIntExclusive area)
        {
            var tiles = new List<Tile>();

            ForEachTile(area, (tile) =>
            {
                if (tile.ContainsObjectOfType(type))
                {
                    tiles.Add(tile);
                }
            });

            return tiles;
        }

        public void TryMoveObject(DungeonObject ob, int newX, int newY)
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

        public void MoveObject(DungeonObject ob, int newX, int newY)
        {
            tileObjects[ob.y][ob.x].RemoveObject(ob);
            tileObjects[newY][newX].AddObject(ob, true);
        }

        public void Reveal(int tileX, int tileY, float radius)
        {
            ForEachTile(t => t.SetInView(false));
            tileObjects[tileY][tileX].SetInView(true);
            Vector2 center = new Vector2(tileX + .5f, tileY + .5f);
            int numRays = 360;
            float stepSize = Mathf.Min(tileWidth, tileHeight) * .9f;
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

                    if (tileObjects[y][wrappedX].isLit)
                    {
                        tileObjects[y][wrappedX].SetInView(true);
                    }
                    if (tileObjects[y][wrappedX].DoesBlockLineOfSight())
                    {
                        break;
                    }
                }
            }
        }

        public void UpdateLighting()
        {
            ForEachTile(t => t.SetLit(false));
            ForEachTile(tile =>
            {
                foreach (var ob in tile.objectList)
                {
                    if (ob.isAlwaysLit)
                    {
                        ob.SetLit(true);
                    }
                    if (ob.illuminationRange != 0)
                    {
                        Vector2 center = new Vector2(tile.x + .5f, tile.y + .5f);
                        int numRays = 360;
                        float stepSize = Mathf.Min(tileWidth, tileHeight) * .9f;
                        for (int r = 0; r < numRays; r++)
                        {
                            float dirX = Mathf.Sin(2 * Mathf.PI * r / numRays);
                            float dirY = Mathf.Cos(2 * Mathf.PI * r / numRays);
                            Vector2 direction = new Vector2(dirX, dirY);

                            for (int d = 1; d < ob.illuminationRange / stepSize; d++)
                            {
                                Vector2 relative = center + direction * d * stepSize;

                                int y = (int)relative.y;
                                if (y < 0 || y >= height) break;

                                int wrappedX = (int)WrapX(relative.x);

                                tileObjects[y][wrappedX].SetLit(true);
                                if (tileObjects[y][wrappedX].DoesBlockLineOfSight())
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            });
        }

        public void ForEachTileThatAllowsSpawn(Action<Tile> doThis, RectIntExclusive area)
        {
            // If the area is larger than the total number of floor tiles it is more efficient to use the precomputed list
            if (area.width * area.height > tilesThatAllowSpawn.Count)
            {
                ForEachTileThatAllowsSpawn(doThis);
                return;
            }
            for (int y = area.yMax; y >= area.yMin; y--)
            {
                for (int x = area.xMax; x >= area.xMin; x--)
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
            for (int i = tilesThatAllowSpawn.Count - 1; i >= 0; i--)
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
}