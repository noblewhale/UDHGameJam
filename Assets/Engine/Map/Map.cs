namespace Noble.TileEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using Random = UnityEngine.Random;

    public class Map : MonoBehaviour
    {

        public BiomeObject biomeObjectPrefab;

        public Biome[] biomeTypes;
        public List<BiomeObject> biomes = new List<BiomeObject>();

        public Tile[][] tiles;
        public List<Tile> tilesThatAllowSpawn = new List<Tile>();

        public int width = 10;
        public int height = 1;

        public Vector2 tileDimensions = new Vector2(1, 1);
        public float tileWidth => tileDimensions.x;
        public float tileHeight => tileDimensions.y;

        public object isDirtyLock = new object();

        [NonSerialized]
        public List<Transform> layers;

        [NonSerialized]
        public Rect totalArea = new Rect();

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

        public event Action OnPreMapLoaded;
        public event Action OnMapLoaded;
        public event Action OnMapGenerationStarted;
        public event Action OnMapCleared;

        public float mapGenerationAnimationDelay = 0;

        public static Map instance;

        public Camera warpedMapCamera;

        public bool isDoneGeneratingMap = false;

        public bool isPremade = true;

        public struct TileAndPosition
        {
            public Tile tile;
            public Vector2 hitPosition;
        }

        virtual public void Awake()
        {
            instance = this;
            layers = GetComponentsInChildren<Tilemap>().Select(tm => tm.transform).ToList();
            if (layers == null || layers.Count == 0) layers = new List<Transform>() { transform };
        }

        virtual public void Start()
        {
            ClearMap();
            StartCoroutine(GenerateMap());

        }

        virtual public void OnDestroy()
        {
            instance = null;
        }

        virtual public void ClearMap()
        {
            isDoneGeneratingMap = false;
            
            ForEachTile(t =>
            {
                t.DestroyAllObjects();
            });

            if (isPremade)
            {
                // Update map area
                var allObs = FindObjectsOfType<DungeonObject>();
                var autoTileObjects = allObs.Where(ob => ob.autoAddToTileAtStart == true);
                foreach (var t in autoTileObjects)
                {
                    if (t.transform.position.x < totalArea.xMin) totalArea.xMin = t.transform.position.x;
                    if (t.transform.position.x > totalArea.xMax) totalArea.xMax = t.transform.position.x;
                    if (t.transform.position.y < totalArea.yMin) totalArea.yMin = t.transform.position.y;
                    if (t.transform.position.y > totalArea.yMax) totalArea.yMax = t.transform.position.y;
                }
                totalArea.xMax += tileWidth;
                totalArea.yMax += tileHeight;
                width = (int)(totalArea.width / tileWidth);
                height = (int)(totalArea.height / tileHeight);
            }

            tiles = new Tile[height][];
            for (int y = 0; y < height; y++) tiles[y] = new Tile[width];
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
            biomes = new List<BiomeObject>(FindObjectsOfType<BiomeObject>());
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

            OnPreMapLoaded?.Invoke();
            OnMapLoaded?.Invoke();
        }

        virtual public void Update()
        {
        }

        virtual public void PlaceBiomes()
        {
            foreach (var biomeType in biomeTypes)
            {
                if (biomeType == null) continue;

                // Make a copy of the boime type so we don't modify properties on the actual asset
                var biomeObjectObject = Instantiate(biomeObjectPrefab.gameObject, transform);
                var biomeObject = biomeObjectObject.GetComponent<BiomeObject>();
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
                //biomeObject.area = new RectIntExclusive(biomeX, biomeY, biomeWidth, biomeHeight);
                biomeObject.area = new RectIntExclusive(0, 0, biomeWidth, biomeHeight);
                biomeObject.biome = biome;
                biomeObject.transform.position = new Vector2(biomeX, biomeY) + (Vector2)transform.position;
                biomes.Add(biomeObject);
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
            tiles[y][x] = new Tile();// Instantiate(tilePrefab, new Vector3(-666, -666, -666), Quaternion.identity, layers[0]).GetComponent<Tile>();
            tiles[y][x].Init(this, x, y);
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
            return Mathf.Clamp(x, totalArea.min.x, totalArea.max.x);
        }

        virtual public float GetYWorldPositionOnMap(float y)
        {
            return Mathf.Clamp(y, totalArea.min.y, totalArea.max.y);
        }

        virtual public Tile GetTile(int x, int y) => GetTile(new Vector2Int(x, y));
        virtual public Tile GetTileFromWorldPosition(float x, float y) => GetTileFromWorldPosition(new Vector2(x, y));

        virtual public Tile GetTile(Vector2Int tilePosition)
        {
            tilePosition = GetTilePositionOnMap(tilePosition);
            if (tilePosition.y < 0 || tilePosition.y >= height || tilePosition.x < 0 || tilePosition.x >= width) return null;
            return tiles[tilePosition.y][tilePosition.x];
        }

        virtual public Tile GetTileFromWorldPosition(Vector2 position)
        {
            position = position - totalArea.min;
            position /= tileDimensions;
            var tilePos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
            return GetTile(tilePos);
        }

        virtual public void ForEachTile(Action<Tile> action)
        {
            if (tiles == null) return;
            if (tiles.Length != height) return;
            for (int y = 0; y < height; y++)
            {
                if (tiles[y].Length != width) return;
                for (int x = 0; x < width; x++)
                {
                    action(tiles[y][x]);
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
            if (!tiles[newY][newX].IsCollidable())
            {
                MoveObject(ob, newX, newY);
            }
            else
            {
                Tile collidedTile = tiles[newY][newX];
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
            if (ob.tile != null)
            {
                GetTile(ob.tilePosition).RemoveObject(ob);
            }
            GetTile(newPos).AddObject(ob, true, -(int)ob.transform.position.z);
        }

        virtual public void MoveObject(DungeonObject ob, int newX, int newY)
        {
            MoveObject(ob, new Vector2Int(newX, newY));
        }

        virtual public void ForEachTileInRadius(Vector2 start, float radius, Action<Tile> action, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, bool showDebug = false)
        {
            if (radius == 0) return;

            var tilesInRadius = GetTilesInRadius(start, radius, stopCondition, includeSourceTile, showDebug);

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

        virtual public List<Tile> GetTilesInRadius(Vector2 start, float radius, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, bool showDebug = false)
        {
            var tileHitsInRadius = GetTileHitsInRadius(start, radius, stopCondition, includeSourceTile, showDebug);
            return tileHitsInRadius.ConvertAll((hit) => hit.tile);
        }

        virtual public List<TileAndPosition> GetTileHitsInRadius(Vector2 start, float radius, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, bool showDebug = false)
        {
            return GetTileHitsInArc(start, radius, 0, Mathf.PI * 2, stopCondition, includeSourceTile, showDebug);
        }

        virtual public List<Tile> GetTilesInArc(Vector2 start, float radius, float arcAngleStart, float arcAngleEnd, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, bool showDebug = false)
        {
            var hits = GetTileHitsInArc(start, radius, arcAngleStart, arcAngleEnd, stopCondition, includeSourceTile, showDebug);
            return hits.ConvertAll(h => h.tile);
        }

        virtual public List<TileAndPosition> GetTileHitsInArc(Vector2 start, float radius, float arcAngleStart, float arcAngleEnd, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, bool showDebug = false)
        {
            var area = new RectIntExclusive(
                (int)(start.x - radius - 1),
                (int)(start.y - radius - 1),
                (int)(radius * 2 + 2 + 1),
                (int)(radius * 2 + 2 + 1)
            );
            var tilesInArea = new List<TileAndPosition>();
            lock (isDirtyLock)
            {
                ForEachTileInArea(area, (t) => { if (t != null) t.isDirty = false; });
                float dif = arcAngleEnd - arcAngleStart;
                float reverseDir = arcAngleStart - arcAngleEnd;
                if (Math.Abs(dif) > Math.Abs(reverseDir))
                {
                    dif = reverseDir;
                }
                float dir = Math.Sign(dif);
                float step = dir * 2 * Mathf.PI / (Mathf.FloorToInt(16 * radius / 4) * 4);
                float maxSteps = Math.Abs(dif) / step;
                for (float r = arcAngleStart, numSteps = 0; r != arcAngleEnd && numSteps < maxSteps; r += step, numSteps++)
                {
                    if (r > Mathf.PI * 2) r -= Mathf.PI * 2;
                    if (r < 0) r += Mathf.PI * 2;
                    Vector2 direction = new Vector2(Mathf.Sin(r), Mathf.Cos(r));

                    GetTileHitsInRay_Dirty(tilesInArea, start, direction, radius, stopCondition, includeSourceTile, .4f, showDebug);
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
            var startTile = GetTileFromWorldPosition(start);
            var area = new RectIntExclusive(
                Mathf.FloorToInt(startTile.x - radius - 1),
                Mathf.FloorToInt(startTile.y - radius - 1),
                Mathf.CeilToInt(radius * 2 + 2 + 1),
                Mathf.CeilToInt(radius * 2 + 2 + 1)
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

                    GetTileHitsInRay_Dirty(tilesInArea, start, direction, truncatedDistance, stopCondition, includeSourceTile, .4f, true);
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
            Tile startTile = GetTileFromWorldPosition(start);
            var area = new RectIntExclusive();
            area.SetMinMax(
                Mathf.Min(Mathf.FloorToInt(startTile.x), Mathf.FloorToInt(startTile.x + dir.x * distance)) - 1,
                Mathf.Max(Mathf.CeilToInt(startTile.x), Mathf.CeilToInt(startTile.x + dir.x * distance)) + 1,
                Mathf.Min(Mathf.FloorToInt(startTile.y), Mathf.FloorToInt(startTile.y + dir.y * distance)) - 1,
                Mathf.Max(Mathf.CeilToInt(startTile.y), Mathf.CeilToInt(startTile.y + dir.y * distance)) + 1
            );
            List<TileAndPosition> tilesInRay = new();
            lock (isDirtyLock)
            {
                ForEachTileInArea(area, (t) => t.isDirty = false);
                GetTileHitsInRay_Dirty(tilesInRay, start, dir, distance, stopCondition, includeSourceTile, stepSize, showDebug);
            }
            return tilesInRay;
        }

        virtual public void GetTileHitsInRay_Dirty(List<TileAndPosition> results, Vector2 start, Vector2 dir, float distance, Func<Tile, bool> stopCondition = null, bool includeSourceTile = false, float stepSize = .4f, bool showDebug = false)
        {
            Vector2 prev = start;
            Vector2 currentPosition;
            Tile startTile = GetTileFromWorldPosition(start);
            Tile currentTile;
            bool wasStopped = false;
            for (int d = 0; d < distance / stepSize; d++)
            {
                currentPosition = start + dir * d * stepSize;

                if (showDebug)
                {
                    Debug.DrawLine(prev, currentPosition, new Color(Random.value, Random.value, Random.value), 4);
                }
                prev = currentPosition;

                if (currentPosition.y < totalArea.yMin || currentPosition.y > totalArea.yMax) break;

                currentTile = GetTileFromWorldPosition(currentPosition);
                if (currentTile == null) continue;
                if (includeSourceTile || currentTile != startTile)
                {
                    if (!currentTile.isDirty)
                    {
                        currentTile.isDirty = true;

                        var hit = new TileAndPosition
                        {
                            tile = currentTile,
                            hitPosition = currentPosition
                        };
                        results.Add(hit);
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
                Vector2 endPos = start + dir * distance;

                if (showDebug)
                {
                    Debug.DrawLine(prev, endPos, new Color(Random.value, Random.value, Random.value), 4);
                }
;
                if (endPos.y > totalArea.yMin && endPos.y < totalArea.yMax)
                {
                    currentTile = GetTileFromWorldPosition(endPos);
                    if (currentTile != null && !currentTile.isDirty && (includeSourceTile || currentTile != startTile))
                    {
                        currentTile.isDirty = true;
                        var hit = new TileAndPosition
                        {
                            tile = currentTile,
                            hitPosition = start + dir * distance
                        };
                        results.Add(hit);
                    }
                }
            }
        }

        virtual public Vector2 GetDifference(Vector2 start, Vector2 end) => new Vector2(GetXDifference(start.x, end.x), GetYDifference(start.y, end.y));

        virtual public Vector2Int GetDifference(Vector2Int start, Vector2Int end) => new Vector2Int(GetXDifference(start.x, end.x), GetYDifference(start.y, end.y));

        virtual public float GetXDifference(float startX, float endX) => endX - startX;

        virtual public float GetYDifference(float startY, float endY) => endY - startY;

        virtual public int GetXDifference(int startX, int endX) => endX - startX;

        virtual public int GetYDifference(int startY, int endY) => endY - startY;

        virtual public void UpdateLighting()
        {
            //ForEachTile(t => t.SetLit(true));
            //ForEachTile(t => t.SetLit(false));
            ForEachTile(t => t.UpdateLighting());
        }

        virtual public void UpdateIsVisible(Tile tile, float radius, bool isVisible)
        {
            tile.SetInView(isVisible);

            ForEachTileInRadius(
                tile.position + tileDimensions / 2,
                radius,
                t => t.SetInView(isVisible),
                t => t.DoesBlockLineOfSight() && t.tilePosition != tile.tilePosition,
                false
            );
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
                    var tile = tiles[y][x];
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