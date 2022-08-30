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

        public Vector2 tileDimensions = new Vector2(1, 1);
        public float tileWidth => tileDimensions.x;
        public float tileHeight => tileDimensions.y;

        public object isDirtyLock = new object();

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

        public struct TileAndPosition
        {
            public Tile tile;
            public Vector2 hitPosition;
        }

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

        virtual public Vector2 GetWorldPositionOnMap(Vector2 pos)
        {
            pos.x = GetXWorldPositionOnMap(pos.x);
            pos.y = GetYWorldPositionOnMap(pos.y);

            return pos;
        }

        virtual public Vector2Int GetTilePositionOnMap(Vector2Int pos)
        {
            pos.x = GetXTilePositionOnMap(pos.x);
            pos.y = GetYTilePositionOnMap(pos.y);

            return pos;
        }

        virtual public int GetXTilePositionOnMap(int x)
        {
            return Math.Clamp(x, 0, width - 1);
        }

        virtual public int GetYTilePositionOnMap(int y)
        {
            return Math.Clamp(y, 0, height - 1);
        }

        virtual public float GetXWorldPositionOnMap(float x)
        {
            return Mathf.Clamp(x, 0, TotalWidth);
        }

        virtual public float GetYWorldPositionOnMap(float y)
        {
            return Mathf.Clamp(y, 0, TotalHeight);
        }

        virtual public Tile GetTile(int x, int y) => GetTile(new Vector2Int(x, y));
        virtual public Tile GetTileFromWorldPosition(float x, float y) => GetTileFromWorldPosition(new Vector2(x, y));

        virtual public Tile GetTile(Vector2Int tilePosition)
        {
            tilePosition = GetTilePositionOnMap(tilePosition);
            return tileObjects[tilePosition.y][tilePosition.x];
        }

        virtual public Tile GetTileFromWorldPosition(Vector2 position)
        {
            position /= tileDimensions;
            var tilePos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
            return GetTile(tilePos);
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
            for (int y = Math.Min(area.yMax, height - 1); y >= Math.Max(area.yMin, 0); y--)
            {
                for (int x = area.xMax; x >= area.xMin; x--)
                {
                    var tile = GetTile(x, y);
                    action(tile);
                }
            }
        }

        virtual public void ForEachTileInPerimeter(RectIntExclusive area, Action<Tile> action)
        {
            Tile tile;
            for (int x = area.xMax; x >= area.xMin; x--)
            {
                tile = GetTile(x, Math.Min(area.yMax, height - 1));
                action(tile);
            }
            for (int y = Math.Min(area.yMax, height - 1); y >= Math.Max(area.yMin, 0); y--)
            {
                tile = GetTile(area.xMin, y);
                action(tile);

                tile = GetTile(area.xMax, y);
                action(tile);
            }
            for (int x = area.xMax; x >= area.xMin; x--)
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

        virtual public void TryMoveObject(DungeonObject ob, Vector2Int newPos)
        {
            TryMoveObject(ob, newPos.x, newPos.y);
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

        virtual public void MoveObject(DungeonObject ob, Vector2Int newPos)
        {
            if (ob.tile)
            {
                GetTile(ob.tilePosition).RemoveObject(ob);
            }
            GetTile(newPos).AddObject(ob, true);
        }

        virtual public void MoveObject(DungeonObject ob, int newX, int newY)
        {
            MoveObject(ob, new Vector2Int(newX, newY));
        }

        virtual public void Reveal(Vector2Int pos, float radius)
        {
            tileObjects[pos.y][pos.x].SetInView(true);

            ForEachTileInRadius(
                pos + tileDimensions / 2,
                radius,
                t => t.SetInView(true),
                t => t.DoesBlockLineOfSight() && t.position != pos
            );
        }

        virtual public void Reveal(int tileX, int tileY, float radius)
        {
            Reveal(new Vector2Int(tileX, tileY), radius);
        }

        virtual public void ForEachTileInRadius(Vector2 start, float radius, Action<Tile> action, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var tilesInRadius = GetTilesInRadius(start, radius, stopCondition, includeSourceTile);

            foreach(var tile in tilesInRadius)
            {
                action(tile);
            }
        }

        virtual public List<Tile> GetTilesInRadiusStraightLines(Vector2 start, float radius, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var tileHitsInRadius = GetTileHitsInRadiusStraightLines(start, radius, stopCondition, includeSourceTile);
            return tileHitsInRadius.ConvertAll((hit) => hit.tile);
        }

        virtual public List<TileAndPosition> GetTileHitsInRadiusStraightLines(Vector2 start, float radius, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var tilesInRadius = GetTileHitsInRadius(start, radius, stopCondition, includeSourceTile);
            tilesInRadius.RemoveAll(
                (hit) => 
                    hit.tile.x != (int)start.x && 
                    hit.tile.y != (int)start.y && 
                    GetXDifference(start.x, hit.tile.x) != (hit.tile.y - start.y) && 
                    GetXDifference(start.x, hit.tile.x) != -(hit.tile.y - start.y)
            );
            return tilesInRadius;
        }

        virtual public List<Tile> GetTilesInRadius(Vector2 start, float radius, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var tileHitsInRadius = GetTileHitsInRadius(start, radius, stopCondition, includeSourceTile);
            return tileHitsInRadius.ConvertAll((hit) => hit.tile);
        }

        virtual public List<TileAndPosition> GetTileHitsInRadius(Vector2 start, float radius, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            return GetTileHitsInArc(start, radius, 0, Mathf.PI * 2, stopCondition, includeSourceTile);
        }

        virtual public List<Tile> GetTilesInArc(Vector2 start, float radius, float arcAngleStart, float arcAngleEnd, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var hits = GetTileHitsInArc(start, radius, arcAngleStart, arcAngleEnd, stopCondition, includeSourceTile);
            return hits.ConvertAll(h => h.tile);
        }

        virtual public List<TileAndPosition> GetTileHitsInArc(Vector2 start, float radius, float arcAngleStart, float arcAngleEnd, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var area = new RectIntExclusive(
                (int)(start.x - radius - 1),
                (int)(start.y - radius - 1),
                (int)(radius * 2 + 2),
                (int)(radius * 2 + 2)
            );
            var tilesInArea = new List<TileAndPosition>();
            lock (isDirtyLock)
            {
                ForEachTileInArea(area, (t) => t.isDirty = false);
                float dif = arcAngleEnd - arcAngleStart;
                float reverseDir = arcAngleStart - arcAngleEnd;
                if (Math.Abs(dif) > Math.Abs(reverseDir))
                {
                    dif = reverseDir;
                }
                float dir = Math.Sign(dif);
                float step = dir * 2 * Mathf.PI / 360;
                float maxSteps = Math.Abs(dif) / step;
                for (float r = arcAngleStart, numSteps = 0; r != arcAngleEnd && numSteps < maxSteps; r += step, numSteps++)
                {
                    if (r > Mathf.PI * 2) r -= Mathf.PI * 2;
                    if (r < 0) r += Mathf.PI * 2;
                    Vector2 direction = new Vector2(Mathf.Sin(r), Mathf.Cos(r));

                    tilesInArea.AddRange(GetTileHitsInRay_Dirty(start, direction, radius, stopCondition, includeSourceTile));
                }
            }

            return tilesInArea;
        }
        virtual public List<Tile> GetTilesInTruncatedArc(Vector2 start, float radius, float arcAngleStart, float arcAngleEnd, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var hits = GetTileHitsInTruncatedArc(start, radius, arcAngleStart, arcAngleEnd, stopCondition, includeSourceTile);
            return hits.ConvertAll(h => h.tile);
        }

        virtual public List<TileAndPosition> GetTileHitsInTruncatedArc(Vector2 start, float radius, float arcAngleStart, float arcAngleEnd, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var area = new RectIntExclusive(
                Mathf.FloorToInt(start.x - radius - 1),
                Mathf.FloorToInt(start.y - radius - 1),
                Mathf.CeilToInt(radius * 2 + 2),
                Mathf.CeilToInt(radius * 2 + 2)
            );
            List<TileAndPosition> tilesInArea = new List<TileAndPosition>();
            lock (isDirtyLock)
            {
                ForEachTileInArea(area, (t) => t.isDirty = false);
                float dif = arcAngleEnd - arcAngleStart;
                float reverseDir = arcAngleStart - arcAngleEnd;
                if (Math.Abs(dif) > Math.Abs(reverseDir))
                {
                    dif = reverseDir;
                }

                float shortDistance = radius * Mathf.Cos(dif / 2);
                float dir = Math.Sign(dif);
                float step = dir * 2 * Mathf.PI / 360;
                float maxSteps = Math.Abs(dif) / step;
                for (float r = arcAngleStart, numSteps = 0; r != arcAngleEnd && numSteps < maxSteps; r += step, numSteps++)
                {
                    if (r > Mathf.PI * 2) r -= Mathf.PI * 2;
                    if (r < 0) r += Mathf.PI * 2;
                    Vector2 direction = new Vector2(Mathf.Sin(r), Mathf.Cos(r));

                    float truncatedDistance = shortDistance / Mathf.Cos(Mathf.Abs((r - arcAngleStart) - dif / 2));

                    tilesInArea.AddRange(GetTileHitsInRay_Dirty(start, direction, truncatedDistance, stopCondition, includeSourceTile, .4f, true));
                }
            }

            return tilesInArea;
        }

        virtual public void ForEachTileInRay(Vector2 start, Vector2 dir, float distance, Action<Tile> action, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false)
        {
            var tilesInRay = GetTileHitsInRay(start, dir, distance, stopCondition, includeSourceTile);

            foreach (var hit in tilesInRay)
            {
                action(hit.tile);
            }
        }

        virtual public List<Tile> GetTilesInRay(Vector2 start, Vector2 end, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, float stepSize = .4f, bool showDebug = false)
        {
            var hits = GetTileHitsInRay(start, end, stopCondition, includeSourceTile, stepSize, showDebug);
            return hits.ConvertAll(h => h.tile);
        }

        virtual public List<Tile> GetTilesInRay(Vector2 start, Vector2 dir, float distance, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, float stepSize = .4f, bool showDebug = false)
        {
            var hits = GetTileHitsInRay(start, dir, distance, stopCondition, includeSourceTile, stepSize, showDebug);
            return hits.ConvertAll(h => h.tile);
        }

        virtual public List<TileAndPosition> GetTileHitsInRay(Vector2 start, Vector2 end, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, float stepSize = .4f, bool showDebug = false)
        {
            Vector2 direction = GetDifference(start, end);
            float distance = direction.magnitude;
            direction.Normalize();

            return GetTileHitsInRay(start, direction, distance, stopCondition, includeSourceTile, stepSize, showDebug);
        }

        virtual public List<TileAndPosition> GetTileHitsInRay(Vector2 start, Vector2 dir, float distance, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, float stepSize = .4f, bool showDebug = false)
        {
            var area = new RectIntExclusive();
            area.SetMinMax(
                Mathf.Min(Mathf.FloorToInt(start.x), Mathf.FloorToInt(start.x + dir.x * distance)),
                Mathf.Max(Mathf.CeilToInt(start.x), Mathf.CeilToInt(start.x + dir.x * distance)),
                Mathf.Min(Mathf.FloorToInt(start.y), Mathf.FloorToInt(start.y + dir.y * distance)),
                Mathf.Max(Mathf.CeilToInt(start.y), Mathf.CeilToInt(start.y + dir.y * distance))
            );
            List<TileAndPosition> tilesInRay;
            lock (isDirtyLock)
            {
                ForEachTileInArea(area, (t) => t.isDirty = false);
                tilesInRay = GetTileHitsInRay_Dirty(start, dir, distance, stopCondition, includeSourceTile, stepSize, showDebug);
            }
            return tilesInRay;
        }

        virtual public List<TileAndPosition> GetTileHitsInRay_Dirty(Vector2 start, Vector2 dir, float distance, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, float stepSize = .4f, bool showDebug = false)
        {
            List<TileAndPosition> tiles = new List<TileAndPosition>();

            Vector2 prev = start;
            Vector2 currentPosition;
            Tile currentTile;
            int y;
            bool wasStopped = false;
            for (int d = 0; d < distance / stepSize; d++)
            {
                currentPosition = start + dir * d * stepSize;

                if (showDebug)
                {
                    Debug.DrawLine((Vector2)transform.position + prev, (Vector2)transform.position + currentPosition, new Color(Random.value, Random.value, Random.value), 4);
                }
                prev = currentPosition;

                y = (int)currentPosition.y;
                if (y < 0 || y >= height) break;

                currentTile = GetTileFromWorldPosition(currentPosition.x, y);

                if (includeSourceTile || currentTile.position != start.ToFloor())
                {
                    if (!currentTile.isDirty)
                    {
                        currentTile.isDirty = true;

                        var hit = new TileAndPosition
                        {
                            tile = currentTile,
                            hitPosition = currentPosition
                        };
                        tiles.Add(hit);
                    }

                    if (stopCondition != null && stopCondition(currentTile))
                    {
                        wasStopped = true;
                        break;
                    }
                }
            }

            if (!wasStopped)
            {
                Vector2 relative = start + dir * distance;
                if (showDebug)
                {
                    Debug.DrawLine((Vector2)transform.position + prev, (Vector2)transform.position + relative, new Color(Random.value, Random.value, Random.value), 4);
                }
                y = (int)relative.y;
                if (y >= 0 && y < height)
                {
                    currentTile = GetTileFromWorldPosition(relative.x, y);

                    if (!currentTile.isDirty && (includeSourceTile || currentTile.position != start.ToFloor()))
                    {
                        currentTile.isDirty = true;
                        var hit = new TileAndPosition
                        {
                            tile = currentTile,
                            hitPosition = start + dir * distance
                        };
                        tiles.Add(hit);
                    }
                }
            }

            return tiles;
        }

        virtual public Vector2 GetDifference(Vector2 start, Vector2 end) => new Vector2(GetXDifference(start.x, end.x), GetYDifference(start.y, end.y));

        virtual public Vector2Int GetDifference(Vector2Int start, Vector2Int end) => new Vector2Int(GetXDifference(start.x, end.x), GetYDifference(start.y, end.y));

        virtual public float GetXDifference(float startX, float endX) => endX - startX;

        virtual public float GetYDifference(float startY, float endY) => endY - startY;

        virtual public int GetXDifference(int startX, int endX) => endX - startX;

        virtual public int GetYDifference(int startY, int endY) => endY - startY;

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

        virtual public void UpdateLighting(Vector2Int center, float radius)
        {
            var area = new RectIntExclusive();
            area.SetToSquare(center, radius + 1);
            UpdateLighting(area);
        }

        virtual public void UpdateLighting(int x, int y, float radius)
        {
            UpdateLighting(new Vector2Int(x, y), radius);
        }

        virtual public void ForEachTileThatAllowsSpawn(Action<Tile> doThis, RectIntExclusive area)
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
            return tilesThatAllowSpawn[Random.Range(0, tilesThatAllowSpawn.Count)];
        }
    }
}