namespace Noble.TileEngine
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using System.Linq;
    
    public class DungeonObject : MonoBehaviour
    {
        public string objectName;

        Creature _creature;
        public Creature Creature
        {
            get {
                if (_creature == null) _creature = GetComponent<Creature>();
                return _creature; 
            }
        }

        public Equipment Equipment => Creature?.Equipment;

        Equipable _equipable;
        public Equipable Equipable {
            get {
                if (_equipable == null) _equipable = GetComponent<Equipable>();
                return _equipable;
            }
        }

        public float illuminationRange = 0;

        [NonSerialized]
        public Vector3 originalGlyphPosition;
        public Map map => Map.instance;

        public Vector2Int tilePosition => tile.tilePosition;

        public int x => tilePosition.x;
        public int y => tilePosition.y;

        [NonSerialized]
        public Tile previousTile;
        public int quantity = 1;
        public bool canBePickedUp;
        public bool canTakeDamage = false;
        public bool isVisibleWhenNotInSight = true;
        public int pathingWeight = 0;
        public Transform guiIcon;
        public bool startHidden = true;
        public float weight = float.MaxValue;

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

        Dictionary<string, IProperty> _properties;
        public Dictionary<string, IProperty> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<string, IProperty>();
                    var propertyComponents = GetComponents<IProperty>();
                    foreach (var property in propertyComponents)
                    {
                        _properties.Add(property.propertyName, property);
                    }
                }
                return _properties;
            }
        }

        public bool isCollidable = true;
        public bool blocksLineOfSight = false;
        public bool coversObjectsBeneath = false;
        public bool preventsObjectSpawning = false;
        bool hasBeenSeen = false;
        
        [SerializeReference]
        public Tile tile;

        public bool autoAddToTileAtStart = true;

        public Tickable tickable { get; private set; }

        [Serializable]
        public class DungeonObjectEvent : UnityEvent<DungeonObject> { }
        [Serializable]
        public class DungeonObjectEvent<T> : UnityEvent<DungeonObject, T> { }
        [Serializable]
        public class DungeonObjectEvent<T1, T2> : UnityEvent<DungeonObject, T1, T2> { }

        [HideInInspector]
        public DungeonObjectEvent OnSteppedOn;
        [HideInInspector]
        public DungeonObjectEvent OnPreSteppedOn;
        [HideInInspector]
        public DungeonObjectEvent<Tile, Tile> onMove;
        [HideInInspector]
        public DungeonObjectEvent<Tile, Tile> onSetPosition;
        [HideInInspector]
        public DungeonObjectEvent<Tile, Tile> onPreMove;
        [HideInInspector]
        public DungeonObjectEvent<Tile, Tile> onPreSetPosition;
        [HideInInspector]
        public DungeonObjectEvent<DungeonObject> onPickedUpObject;
        [HideInInspector]
        public DungeonObjectEvent<DungeonObject> onPickedUp;
        [HideInInspector]
        public DungeonObjectEvent<bool> onCollision;
        [HideInInspector]
        public DungeonObjectEvent onDeath;
        [HideInInspector]
        public DungeonObjectEvent onSpawn;
        [HideInInspector]
        public DungeonObjectEvent<int> onTakeDamage;

        virtual protected void Awake()
        {
            tickable = GetComponent<Tickable>();
            glyphs = GetComponentInChildren<Glyphs>(true);
            if (glyphs)
            {
                glyphsOb = glyphs.gameObject;
                originalGlyphPosition = glyphs.transform.localPosition;
            }
            if (!startHidden) hasBeenSeen = true;
            SetInView(false, false);
        }

        virtual protected void Start()
        { 
            map.OnPreMapLoaded += OnPreMapLoaded;
        }

        public void OnPreMapLoaded()
        {
            if (tile == null && autoAddToTileAtStart && GetComponentInParent<Map>())
            {
                tile = map.GetTileFromWorldPosition(transform.position);
                if (tile != null)
                {
                    tile.AddObject(this, false, (int)-transform.position.z);
                }
            }
        }

        public void Spawn()
        {
            onSpawn?.Invoke(this);
        }

        public Property<T> GetProperty<T>(string propertyName)
        {
            Properties.TryGetValue(propertyName, out IProperty prop);
            return (Property<T>)prop;
        }

        public T GetPropertyValue<T>(string propertyName)
        {
            Properties.TryGetValue(propertyName, out IProperty prop);
            if (prop == null) return default(T);
            else return ((Property<T>)prop).GetValue();
        }

        public void UpdateLighting()
        {
            if (tile == null) return;
            if (illuminationRange == 0) return;

            if (illuminationRange < .5f)
            {
                tile.AddIlluminationSource();
                return;
            }

            map.ForEachTileInRadius(
                tile.position + map.tileDimensions / 2,
                illuminationRange, 
                (Tile t) => 
                {
                    t.AddIlluminationSource();
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
            onCollision?.Invoke(ob, isInstigator);
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
            glyphs.DamageFlash(animationTime);
        }

        public void TakeDamage(int v)
        {
            if (!canTakeDamage) return;

            onTakeDamage.Invoke(this, v);
        }

        virtual public void Die()
        {
            if (onDeath != null) onDeath.Invoke(this);
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
            if (onPickedUpObject != null) onPickedUpObject.Invoke(this, objectToPickUp);
            if (objectToPickUp.onPickedUp != null) objectToPickUp.onPickedUp.Invoke(objectToPickUp, this);
        }

        public void Move(Vector2Int pos)
        {
            SetPosition(pos, true);
        }

        public void SetPosition(Vector2Int position, bool isMove = false)
        {
            var newTile = map.GetTile(position);

            if (tile != null)
            {
                if (isMove && onPreMove != null) onPreMove.Invoke(this, tile, newTile);
                onPreSetPosition?.Invoke(this, tile, newTile);
            }

            previousTile = tile;
            tile = newTile;

            if (isMove)
            {
                if (previousTile != null)
                {
                    map.ForEachTileInRadius(
                        previousTile.position + map.tileDimensions / 2,
                        illuminationRange,
                        t => t.RemoveIlluminationSource(),
                        null,
                        true
                    );
                }
                UpdateLighting();
            }


            if (isMove && onMove != null) onMove.Invoke(this, previousTile, tile);
            if (onSetPosition != null) onSetPosition.Invoke(this, previousTile, tile);
        }
    }
}