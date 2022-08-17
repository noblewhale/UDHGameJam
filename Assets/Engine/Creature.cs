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

        public int x { get { return baseObject.x; } }
        public int y { get { return baseObject.y; } }
        public Map map { get { return baseObject.map; } }
        public Inventory inventory { get { return baseObject.inventory; } }

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

        void OnMove(int oldX, int oldY, int newX, int newY)
        {
            lastDirectionAttackedOrMoved = GetDirection(oldX, oldY, newX, newY);

            map.tileObjects[newY][newX].StepOn(baseObject);
        }

        // TODO: Wrapping
        public Direction GetDirection(int oldX, int oldY, int newX, int newY)
        {
            int xDif = oldX - newX;
            int yDif = oldY - newY;
            if (Math.Abs(xDif) == Math.Abs(yDif))
            {
                if (xDif > 0 && yDif > 0) return Direction.UP_RIGHT;
                else if (xDif > 0 && yDif < 0) return Direction.DOWN_RIGHT;
                else if (xDif < 0 && yDif < 0) return Direction.DOWN_LEFT;
                else return Direction.UP_LEFT;
            }
            else if (Math.Abs(xDif) > Math.Abs(yDif))
            {
                if (xDif > 0) return Direction.RIGHT;
                else return Direction.LEFT;
            }
            else
            {
                if (yDif > 0) return Direction.UP;
                else return Direction.DOWN;
            }
        }

        public void WeildRightHand(DungeonObject ob)
        {
            Weildable oldWieldable = rightHandObject?.GetComponent<Weildable>();
            Weildable newWeildable = ob?.GetComponent<Weildable>();

            oldWieldable?.GetComponent<Weildable>().UnWeild();
            if (rightHandObject)
            {
                baseObject.glyphs.glyphs.RemoveAll(g => rightHandObject.glyphs.glyphs.Contains(g));
            }

            rightHandObject = ob;
            newWeildable?.Weild(this, rightHand);
            if (rightHandObject)
            {
                baseObject.glyphs.glyphs.AddRange(ob.glyphs.glyphs);
            }
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