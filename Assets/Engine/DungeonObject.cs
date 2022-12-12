namespace Noble.TileEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using System.Linq;
    using UnityEngine.Tilemaps;

    public class DungeonObject : MonoBehaviour
    {
        public string objectName;

        [Serializable]
        public class CreatureEvent : UnityEvent<DungeonObject> { }

        public float illuminationRange = 0;
        public CreatureEvent OnSteppedOn;
        public CreatureEvent OnPreSteppedOn;
        public Vector3 originalGlyphPosition;
        public Map map;

        public Vector2Int tilePosition => tile.tilePosition;

        public int x => tilePosition.x;
        public int y => tilePosition.y;

        public Vector2Int previousTilePosition;
        public int quantity = 1;
        public bool canBePickedUp;
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
        bool hasBeenSeen = false;
        public bool hasDoneDamageFlash = false;

        public event Action<Vector2Int, Vector2Int> onMove;
        public event Action<Vector2Int, Vector2Int> onSetPosition;
        public event Action<Vector2Int, Vector2Int> onPreMove;
        public event Action<Vector2Int, Vector2Int> onPreSetPosition;
        public event Action<DungeonObject> onPickedUpObject;
        public event Action<DungeonObject, bool> onCollision;
        public event Action onDeath;
        public event Action onSpawn;
        public Tile tile;
        public bool autoAddToTileAtStart = true;

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

            map.OnPreMapLoaded += OnPreMapLoaded;
        }

        public void OnPreMapLoaded()
        {
            if (tile == null && autoAddToTileAtStart)
            {
                tile = map.GetTileFromWorldPosition(transform.position - map.transform.position);
                if (tile != null)
                {
                    tile.AddObject(this, false, (int)-transform.position.z);
                }
            }
        }

        public void Spawn()
        {
            onSpawn?.Invoke();
        }

        public void UpdateLighting()
        {
            if (illuminationRange == 0) return;

            if (illuminationRange < .5f)
            {
                tile.SetLit(true);
                return;
            }

            map.ForEachTileInRadius(
                tilePosition + map.tileDimensions / 2,
                illuminationRange, 
                (Tile t) => 
                {
                    t.SetLit(true);
                },
                (Tile t) =>
                {
                    return t.DoesBlockLineOfSight() && (t != tile);
                },
                true
            );
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

            glyphs?.SetLit(isLit);

            if (isLit)
            {
                if ((isVisibleWhenNotInSight || tile.isInView) && hasBeenSeen)
                {
                    glyphs?.SetRevealed(true);
                }
            }
            else
            {
                if (!isVisibleWhenNotInSight)
                {
                    glyphs?.SetRevealed(false);
                }    
            }
        }

        public void CollideWith(DungeonObject ob, bool isInstigator)
        {
            if (onCollision != null) onCollision(ob, isInstigator);
        }

        public void SteppedOn(DungeonObject creature)
        {
            OnSteppedOn?.Invoke(creature);
        }

        public void PreSteppedOn(DungeonObject creature)
        {
            OnPreSteppedOn?.Invoke(creature);
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
            tile.objectList.Remove(this);
            DropItems();
            Destroy(gameObject);
        }

        virtual public void DropItems()
        {
            foreach (var kv in inventory.items)
            {
                tile.AddObject(kv.Value);
            }
        }

        public void PickUpAll()
        {
            foreach (var ob in tile.objectList.Reverse())
            {
                if (ob.canBePickedUp)
                {
                    tile.RemoveObject(ob);
                    AddToInventory(ob);
                }
            }
        }

        public void AddToInventory(DungeonObject objectToPickUp)
        {
            DungeonObject existingOb;
            bool success = inventory.items.TryGetValue(objectToPickUp.objectName, out existingOb);
            if (success)
            {
                existingOb.quantity += objectToPickUp.quantity;
            }
            else
            {
                inventory.items.Add(objectToPickUp.objectName, objectToPickUp);
            }
            objectToPickUp.transform.position = new Vector3(-666, -666, -666);
            if (onPickedUpObject != null) onPickedUpObject(objectToPickUp);
        }

        public void Move(Vector2Int pos)
        {
            SetPosition(pos, true);
        }

        public void SetPosition(Vector2Int position, bool isMove = false)
        {
            if (isMove && onPreMove != null) onPreMove(tilePosition, position);
            if (onPreSetPosition != null) onPreSetPosition(tilePosition, position);

            tile = map.GetTile(position);

            if (isMove && onMove != null) onMove(previousTilePosition, tilePosition);
            if (onSetPosition != null) onSetPosition(previousTilePosition, tilePosition);

            previousTilePosition = position;
        }
    }
}