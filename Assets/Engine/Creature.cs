namespace Noble.TileEngine
{
    using System;
    using UnityEngine;

    [RequireComponent(typeof(Tickable))]
    [RequireComponent(typeof(DungeonObject))]
    public class Creature : MonoBehaviour
    {
        //public Race race;

        public int mana;
        public int strength;
        public int dexterity;
        public int constitution;
        public int intelligence;
        public int charisma;

        public int defense = 1;

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

        public Direction lastDirectionAttackedOrMoved = Direction.UP;

        public Coroutine attackAnimationProcess;

        public DungeonObject leftHandObject;
        public DungeonObject rightHandObject;
        public Transform rightHand;
        public Transform leftHand;
        public Transform chest;
        public Transform helmet;
        public Transform pants;

        public DungeonObject baseObject;
        public Tickable tickable;

        public Vector2Int tilePosition => baseObject.tilePosition;
        public int x => baseObject.x;
        public int y => baseObject.y;
        public Map map => baseObject.map;
        public Inventory inventory => baseObject.inventory;

        public int health { get { return baseObject.health; } }
        public int gold { get { return baseObject.gold; } }

        public float attackAnimationTime = 0;
        public float attackAnimationDuration = .5f;
        public float attackAnimationScale = 1;
        //bool attackWillHit = false;

        void Awake()
        {
            baseObject = GetComponent<DungeonObject>();
            tickable = GetComponent<Tickable>();
            baseObject.onMove += OnMove;
            baseObject.onPickedUpObject += OnPickedUpObject;
        }

        void OnPickedUpObject(DungeonObject ob)
        {
            if (ob.GetComponent<Weapon>() != null)
            {
                WeildRightHand(ob);
            }
        }

        void OnMove(Vector2Int oldPos, Vector2Int newPos)
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

        public void WeildRightHand(DungeonObject ob)
        {
            Weildable oldWieldable = rightHandObject?.GetComponent<Weildable>();
            Weildable newWeildable = ob?.GetComponent<Weildable>();

            oldWieldable?.GetComponent<Weildable>().UnWeild();
            //if (rightHandObject)
            //{
            //    baseObject.glyphs.glyphs.RemoveAll(g => rightHandObject.glyphs.glyphs.Contains(g));
            //}

            rightHandObject = ob;
            rightHandObject.SetLit(true, true);
            newWeildable?.Weild(this, rightHand);
            //if (rightHandObject)
            //{
            //    baseObject.glyphs.glyphs.AddRange(ob.glyphs.glyphs);
            //}
        }

        public void FaceDirection(Tile tile)
        {
            if (tile.y > y) lastDirectionAttackedOrMoved = Direction.UP;
            if (tile.y < y) lastDirectionAttackedOrMoved = Direction.DOWN;
            if (tile.x > x) lastDirectionAttackedOrMoved = Direction.RIGHT;
            if (tile.x < x) lastDirectionAttackedOrMoved = Direction.LEFT;
        }

    }
}