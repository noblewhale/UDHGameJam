namespace Noble.TileEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class DungeonObject : MonoBehaviour
    {
        public string objectName;

        [Serializable]
        public class CreatureEvent : UnityEvent<DungeonObject> { }

        public float viewDistance = 4;
        public float illuminationRange = 0;
        public CreatureEvent OnSteppedOn;
        public Vector3 originalGlyphPosition;
        public Map map;

        [SerializeField]
        Vector2Int position = Vector2Int.zero;
        public int x
        {
            get => position.x;
            set => position.x = value;
        }

        public int y
        {
            get => position.y;
            set => position.y = value;
        }

        public int previousX, previousY;
        public int quantity = 1;
        public bool canBePickedUp;
        public bool isAlwaysLit;
        public bool canTakeDamage = false;
        public bool isVisibleWhenNotInSight = true;
        public int pathingWeight = 0;
        public Transform guiIcon;

        [NonSerialized]
        public Glyphs glyphs;
        [NonSerialized]
        public GameObject glyphsOb;

        public Inventory inventory = new Inventory();

        public int gold
        {
            get
            {
                return inventory.Gold;
            }
        }

        public int health = 1;
        public bool isCollidable = true;
        public bool blocksLineOfSight = false;
        public bool coversObjectsBeneath = false;
        public bool preventsObjectSpawning = false;
        public bool hasBeenSeen = false;
        public bool hasDoneDamageFlash = false;

        public event Action<int, int, int, int> onMove;
        public event Action<int, int, int, int> onSetPosition;
        public event Action<DungeonObject> onPickedUpObject;
        public event Action<DungeonObject, bool> onCollision;
        public event Action onDeath;
        public event Action onSpawn;
        public Tile tile;

        public Tickable tickable { get; private set; }

        virtual protected void Awake()
        {
            tickable = GetComponent<Tickable>();
            map = FindObjectOfType<Map>();
            glyphs = GetComponentInChildren<Glyphs>();
            if (glyphs)
            {
                glyphsOb = glyphs.gameObject;
                originalGlyphPosition = glyphs.transform.localPosition;
            }
        }

        public void Spawn()
        {
            onSpawn?.Invoke();
        }

        public void UpdateLighting()
        {
            if (illuminationRange == 0) return;

            Vector2 center = new Vector2(x + .5f, y + .5f);
            int numRays = 360;
            float stepSize = Mathf.Min(map.tileWidth, map.tileHeight) * .9f;

            var area = new RectIntExclusive(
                (int)(x - illuminationRange / 2 - 1),
                (int)(y - illuminationRange / 2 - 1),
                (int)(illuminationRange + 3),
                (int)(illuminationRange + 3)
            );

            map.ForEachTile(area, (t) => t.isDirty = false);
            for (int r = 0; r < numRays; r++)
            {
                float dirX = Mathf.Sin(2 * Mathf.PI * r / numRays);
                float dirY = Mathf.Cos(2 * Mathf.PI * r / numRays);
                Vector2 direction = new Vector2(dirX, dirY);

                for (int d = 1; d < illuminationRange / stepSize; d++)
                {
                    Vector2 relative = center + direction * d * stepSize;

                    int y = (int)relative.y;
                    if (y < 0 || y >= map.height) break;

                    int wrappedX = (int)map.WrapX(relative.x);

                    if (!map.tileObjects[y][wrappedX].isDirty)
                    {
                        map.tileObjects[y][wrappedX].isDirty = true;
                        map.tileObjects[y][wrappedX].SetLit(true);
                    }

                    if (map.tileObjects[y][wrappedX].DoesBlockLineOfSight() && (y != tile.y || wrappedX != tile.x))
                    {
                        break;
                    }
                }
            }
        }

        public void SetInView(bool isInView, bool reveal)
        {
            if (isInView && reveal) hasBeenSeen = true;

            if (isInView)
            {
                if (hasBeenSeen)
                {
                    glyphs.SetRevealed(true);
                }
            }
            else
            {
                if (!isVisibleWhenNotInSight || !hasBeenSeen)
                {
                    if (glyphs) glyphs.SetRevealed(false);
                }
            }
        }

        public void SetLit(bool isLit, bool reveal)
        {
            if (isLit && reveal) hasBeenSeen = true;
            if (isAlwaysLit) isLit = true;

            if (glyphs) glyphs.SetLit(isLit);
            if (hasBeenSeen)
            {
                glyphs.SetRevealed(true);
            }
        }

        public void CollideWith(DungeonObject ob, bool isInstigator)
        {
            if (onCollision != null) onCollision(ob, isInstigator);
        }

        public void SteppedOn(DungeonObject creature)
        {
            OnSteppedOn.Invoke(creature);
        }

        public void DamageFlash(float animationTime)
        {
            hasDoneDamageFlash = true;
            glyphs.DamageFlash(animationTime);
        }

        public void TakeDamage(int v)
        {
            if (!canTakeDamage) return;

            hasDoneDamageFlash = false;
            health -= v;

            if (health < 0) health = 0;
            if (health == 0)
            {
                Die();
            }
        }

        virtual public void Die()
        {
            if (onDeath != null) onDeath();
            map.tileObjects[y][x].objectList.Remove(this);
            DropItems();
            Destroy(gameObject);
        }

        virtual public void DropItems()
        {
            foreach (var kv in inventory.items)
            {
                map.tileObjects[y][x].AddObject(kv.Value);
            }
        }

        public void PickUpAll()
        {
            List<DungeonObject> itemsToRemoveFromTile = new List<DungeonObject>();
            List<DungeonObject> itemsToDestroy = new List<DungeonObject>();
            foreach (var ob in map.tileObjects[y][x].objectList)
            {
                if (ob.canBePickedUp)
                {
                    itemsToRemoveFromTile.Add(ob);
                    DungeonObject existingOb;
                    bool success = inventory.items.TryGetValue(ob.objectName, out existingOb);
                    if (success)
                    {
                        existingOb.quantity += ob.quantity;
                        itemsToDestroy.Add(ob);
                    }
                    else
                    {
                        inventory.items.Add(ob.objectName, ob);
                    }
                    ob.transform.position = new Vector3(-666, -666, -666);
                }
            }

            foreach (var ob in itemsToRemoveFromTile)
            {
                map.tileObjects[y][x].RemoveObject(ob);
                if (onPickedUpObject != null) onPickedUpObject(ob);
            }

            foreach (var ob in itemsToDestroy) Destroy(ob);
        }

        public void Move(int newX, int newY)
        {
            SetPosition(newX, newY);

            if (onMove != null) onMove(previousX, previousY, x, y);
        }

        public void SetPosition(int newX, int newY)
        {
            previousX = x;
            previousY = y;
            x = newX;
            y = newY;
            tile = map.tileObjects[y][x];

            //map.UpdateLighting();

            if (onSetPosition != null) onSetPosition(previousX, previousY, x, y);
        }
    }
}