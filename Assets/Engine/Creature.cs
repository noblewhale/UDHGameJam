namespace Noble.TileEngine
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [RequireComponent(typeof(Tickable))]
    [RequireComponent(typeof(DungeonObject))]
    public class Creature : MonoBehaviour
    {
        public float viewDistance = 4;

        [SerializeField]
        public float effectiveViewDistance
        {
            get
            {
                float d = viewDistance;
                foreach (ViewDistanceModifier modifier in viewDistanceModifiers)
                {
                    modifier.ModifyViewDistance(ref d);
                }
                return d;
            }
        }
        public List<ViewDistanceModifier> viewDistanceModifiers = new List<ViewDistanceModifier>();
        List<Modifier> allModifier = new List<Modifier>();

        public ulong ticksPerMove = 1;
        public ulong ticksPerAttack = 1;
        public AnimationCurve attackMovementAnimation;
        public float Speed
        {
            get
            {
                if (ticksPerMove == 0) return 0;
                return 1 / ticksPerMove;
            }
        }

        [NonSerialized]
        public Direction lastDirectionAttackedOrMoved = Direction.UP;

        public Coroutine attackAnimationProcess;

        Equipment _equipment;
        public Equipment Equipment { 
            get { 
                if (_equipment == null) _equipment = GetComponent<Equipment>(); 
                return _equipment; 
            } 
        }

        [NonSerialized]
        public DungeonObject baseObject;
        [NonSerialized]
        public Tickable tickable;

        public Vector2Int tilePosition => baseObject.tilePosition;
        public int x => baseObject.x;
        public int y => baseObject.y;
        public Map map => baseObject.map;
        public Inventory inventory => baseObject.inventory;

        public int gold { get { return baseObject.gold; } }

        [NonSerialized]
        public float attackAnimationTime = 0;

        public float attackAnimationDuration = .5f;
        public float attackAnimationScale = 1;

        MentalMap mentalMap;

        void Awake()
        {
            baseObject = GetComponent<DungeonObject>();
            tickable = GetComponent<Tickable>();
            baseObject.onMove.AddListener(OnMove);
            baseObject.onPreMove.RemoveListener(OnPreMove);
        }

        private void Start()
        {
            mentalMap = new MentalMap();
        }

        void OnPreMove(DungeonObject _, Vector2Int pos, Vector2Int newPos)
        {
            map.GetTile(newPos).PreStepOn(baseObject);
        }

        void OnMove(DungeonObject _, Vector2Int oldPos, Vector2Int newPos)
        {
            lastDirectionAttackedOrMoved = GetDirection(oldPos, newPos);

            map.GetTile(newPos).StepOn(baseObject);
        }

        public Direction GetDirection(Vector2Int start, Vector2Int end)
        {
            return GetDirection(start.x, start.y, end.x, end.y);
        }

        public Direction GetDirection(int startX, int startY, int endX, int endY)
        {
            Vector2Int dif = Map.instance.GetDifference(new Vector2Int(startX, startY), new Vector2Int(endX, endY));
            if (Math.Abs(dif.x) == Math.Abs(dif.y))
            {
                if (dif.x > 0 && dif.y > 0) return Direction.UP_RIGHT;
                else if (dif.x > 0 && dif.y < 0) return Direction.DOWN_RIGHT;
                else if (dif.x < 0 && dif.y < 0) return Direction.DOWN_LEFT;
                else return Direction.UP_LEFT;
            }
            else if (Math.Abs(dif.x) > Math.Abs(dif.y))
            {
                if (dif.x > 0) return Direction.RIGHT;
                else return Direction.LEFT;
            }
            else
            {
                if (dif.y > 0) return Direction.UP;
                else return Direction.DOWN;
            }
        }

        public void FaceDirection(Tile tile)
        {
            if (tile.y > y) lastDirectionAttackedOrMoved = Direction.UP;
            if (tile.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
            if (tile.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
            if (tile.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
        }

        public void AddModifier<T>() where T : Modifier
        {
            var modifier = gameObject.AddComponent<T>();
            if (modifier is ViewDistanceModifier)
            {
                viewDistanceModifiers.Add((ViewDistanceModifier)modifier);
            }
            allModifier.Add(modifier);
            tickable.behaviours.Add(modifier);
        }

        public void RemoveModifier(Modifier modifier)
        {
            if (modifier is ViewDistanceModifier)
            {
                viewDistanceModifiers.Remove((ViewDistanceModifier)modifier);
            }
            allModifier.Remove(modifier);
            tickable.behaviours.Remove(modifier);

            Destroy(modifier);
        }


        public Equipable GetEquipment(Equipment.Slot slot)
        {
            return Equipment?.GetEquipment(slot);
        }

        public void SetEquipment(Equipment.Slot slot, Equipable item)
        {
            Equipment?.SetEquipment(slot, item);
        }

        public Transform GetEquipmentSlot(Equipment.Slot slot) 
        {
            return Equipment?.GetSlotTransform(slot);
        }
    }
}