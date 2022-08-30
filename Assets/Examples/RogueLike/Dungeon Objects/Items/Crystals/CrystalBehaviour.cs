namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class CrystalBehaviour : TickableBehaviour
    {
        public enum CrystalType
        {
            FIRE=1, WATER=2, WIND=3
        }

        public CrystalType[] powerParts;
        public CrystalType[] shapeParts;

        [SerializeField]
        [Range(1, 3)]
        private int level = 1;
        public int Level => level;

        PowerBehaviour powerBehaviour;
        TargetableBehaviour shapeBehaviour;

        private void Start()
        {
            // Look up power and shape based on rune parts
            string power = RogueDB.instance.GetPowerFromParts(powerParts);
            string shape = RogueDB.instance.GetShapeFromParts(shapeParts);

            // Add power and shape behaviors
            var powerPrefab = Resources.Load<GameObject>(CharsToTitleCase(power + "Power"));
            var powerObject = Instantiate(powerPrefab, transform);
            powerBehaviour = powerObject.GetComponent<PowerBehaviour>();
            var shapePrefab = Resources.Load<GameObject>(CharsToTitleCase(shape + "Shape"));
            var shapeObject = Instantiate(shapePrefab, transform);
            shapeBehaviour = shapeObject.GetComponent<TargetableBehaviour>();

            // Power needs to know about shape to get threatened tiles
            powerBehaviour.shapeBehaviour = shapeBehaviour;

            // Tell the shape behaviour which shape sprite to use to determine which tiles to threaten
            var shapes = Resources.LoadAll<Sprite>("Rune Shapes/shapes");
            foreach (var shapeTexture in shapes)
            {
                if (shapeTexture.name == shape + "-" + level)
                {
                    shapeBehaviour.shapeSprite = shapeTexture;
                    break;
                }
            }

            // Set the range for the shape behaviour
            float range = RogueDB.instance.GetRange(shape, level);
            shapeBehaviour.aimingRadius = range;

            // Set the min and max damage per tile for the power behaviour
            (float min, float max) = CalculateMinMaxDamage();
            powerBehaviour.minDamage = min;
            powerBehaviour.maxDamage = max;
        }

        (float, float) CalculateMinMaxDamage()
        {
            if (powerParts.Length == 1 && shapeParts.Length == 0)
            {
                return RogueDB.instance.GetMinMaxDamage((int)powerParts[0], level);
            }
            else if (powerParts.Length == 1 && shapeParts.Length == 1)
            {
                (float p1Min, float p1Max) = RogueDB.instance.GetMinMaxDamage((int)powerParts[0], level);
                (float s1Min, float s1Max) = RogueDB.instance.GetMinMaxDamage((int)shapeParts[0], level);
                // Combine the damages, favoring the power rune
                float min = p1Min * .75f + s1Min * .25f;
                float max = p1Max * .75f + s1Max * .25f;
                // Scale the damage up because it's a combined rune
                min *= 1.5f;
                max *= 1.5f;
                // If either rune is a fire rune, round minimum down to increase wildness
                if (powerParts[0] == CrystalType.FIRE || shapeParts[0] == CrystalType.FIRE)
                {
                    min = Mathf.Floor(min);
                }
                // Otherwise round minimum up
                else
                {
                    min = Mathf.Floor(min);
                }
                // Always round max up
                max = Mathf.Ceil(max);

                return (min, max);
            }
            else if (powerParts.Length == 2 && shapeParts.Length == 1)
            {
                (float p1Min, float p1Max) = RogueDB.instance.GetMinMaxDamage((int)powerParts[0], level);
                (float p2Min, float p2Max) = RogueDB.instance.GetMinMaxDamage((int)powerParts[1], level);
                (float s1Min, float s1Max) = RogueDB.instance.GetMinMaxDamage((int)shapeParts[0], level);
                // Combine the damages, favoring the power runes
                float min = p1Min * .5f + p2Min * .4f + s1Min * .1f;
                float max = p1Max * .5f + p2Max * .4f + s1Max * .1f;
                // Scale the damage up because it's a double combined rune
                min *= 3f;
                max *= 3f;
                // If any rune is a fire rune, round minimum down to increase wildness
                if (powerParts[0] == CrystalType.FIRE || powerParts[1] == CrystalType.FIRE || shapeParts[0] == CrystalType.FIRE)
                {
                    min = Mathf.Floor(min);
                }
                // Otherwise round minimum up
                else
                {
                    min = Mathf.Floor(min);
                }
                // Always round max up
                max = Mathf.Ceil(max);

                return (min, max);
            }
            else if (powerParts.Length == 1 && shapeParts.Length == 2)
            {
                (float p1Min, float p1Max) = RogueDB.instance.GetMinMaxDamage((int)powerParts[0], level);
                (float s1Min, float s1Max) = RogueDB.instance.GetMinMaxDamage((int)shapeParts[0], level);
                (float s2Min, float s2Max) = RogueDB.instance.GetMinMaxDamage((int)shapeParts[1], level);
                // Combine the damages, favoring the power rune
                float min = p1Min * .5f + s1Min * .3f + s2Min * .2f;
                float max = p1Max * .5f + s1Max * .3f + s2Max * .2f;
                // Scale the damage up because it's a double combined rune
                min *= 3f;
                max *= 3f;
                // If any rune is a fire rune, round minimum down to increase wildness
                if (powerParts[0] == CrystalType.FIRE || powerParts[1] == CrystalType.FIRE || shapeParts[0] == CrystalType.FIRE)
                {
                    min = Mathf.Floor(min);
                }
                // Otherwise round minimum up
                else
                {
                    min = Mathf.Floor(min);
                }
                // Always round max up
                max = Mathf.Ceil(max);

                return (min, max);
            }
            else
            {
                throw new Exception("Invalid rune combination");
            }
        }

        string CharsToTitleCase(string s)
        {
            bool newWord = true;
            char[] asChars = s.ToCharArray();
            for (int ci = 0; ci < s.Length; ci++)
            {
                if (newWord) { asChars[ci] = char.ToUpper(asChars[ci]); newWord = false; }
                else asChars[ci] = char.ToLower(asChars[ci]);
                if (asChars[ci] == ' ') newWord = true;
            }
            s = new string(asChars);
            return s.Replace(" ", "");
        }


        public override bool IsActionACoroutine() => true;

        public override IEnumerator StartActionCoroutine()
        {
            yield return shapeBehaviour.StartActionCoroutine();
            yield return powerBehaviour.StartActionCoroutine();
        }

        public override void StartAction()
        {
            powerBehaviour.StartAction();
        }

        public override void StartSubAction(ulong time)
        {
            powerBehaviour.StartSubAction(time);
        }

        public override bool ContinueSubAction(ulong time)
        {
            return powerBehaviour.ContinueSubAction(time);
        }

        public override void FinishSubAction(ulong time)
        {
            powerBehaviour.FinishSubAction(time);
        }

        public override void FinishAction()
        {
            powerBehaviour.FinishAction();
        }
    }
}