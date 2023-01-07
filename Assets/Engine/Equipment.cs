namespace Noble.TileEngine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Equipment : MonoBehaviour
    {

        public Equipable leftHandWeaponObject;
        public Equipable rightHandWeaponObject;
        public Equipable glovesObject;
        public Equipable leftHandObject;
        public Equipable rightHandObject;
        public Equipable chestObject;
        public Equipable headObject;
        public Equipable legsObject;
        public Equipable feetObject;
        public Equipable twoHandedObject;
        public Transform rightHandWeaponSlot;
        public Transform leftHandWeaponSlot;
        public Transform glovesSlot;
        public Transform leftHandSlot;
        public Transform rightHandSlot;
        public Transform chestSlot;
        public Transform headSlot;
        public Transform legsSlot;
        public Transform feetSlot;
        public Transform twoHandedSlot;

        public event Action onChange;

        public enum Slot
        {
            LEFT_HAND_WEAPON,
            RIGHT_HAND_WEAPON,
            GLOVES,
            CHEST,
            HEAD,
            LEGS,
            FEET,
            TWO_HANDED,
            LEFT_HAND,
            RIGHT_HAND
        }

        public IEnumerable<Equipable> GetEquipment()
        {
            yield return leftHandWeaponObject;
            yield return rightHandWeaponObject;
            yield return glovesObject;
            yield return chestObject;
            yield return headObject;
            yield return legsObject;
            yield return feetObject;
            yield return twoHandedObject;
            yield return leftHandObject;
            yield return rightHandObject;
        }

        public Transform GetSlotTransform(Slot slot)
        {
            switch (slot)
            {
                case Slot.LEFT_HAND_WEAPON: return leftHandWeaponSlot;
                case Slot.RIGHT_HAND_WEAPON: return rightHandWeaponSlot;
                case Slot.GLOVES: return glovesSlot;
                case Slot.CHEST: return chestSlot;
                case Slot.HEAD: return headSlot;
                case Slot.LEGS: return legsSlot;
                case Slot.FEET: return feetSlot;
                case Slot.TWO_HANDED: return twoHandedSlot;
                case Slot.LEFT_HAND: return leftHandSlot;
                case Slot.RIGHT_HAND: return rightHandSlot;
                default: return null;
            }
        }

        public Equipable GetEquipment(Slot slot)
        {
            switch (slot)
            {
                case Slot.LEFT_HAND_WEAPON: return leftHandWeaponObject;
                case Slot.RIGHT_HAND_WEAPON: return rightHandWeaponObject;
                case Slot.GLOVES: return glovesObject;
                case Slot.CHEST: return chestObject;
                case Slot.HEAD: return headObject;
                case Slot.LEGS: return legsObject;
                case Slot.FEET: return feetObject;
                case Slot.TWO_HANDED: return twoHandedObject;
                case Slot.LEFT_HAND: return leftHandObject;
                case Slot.RIGHT_HAND: return rightHandObject;
                default: return null;
            }
        }

        public void SetEquipment(Slot slot, Equipable item)
        {
            if (item != null)
            {
                var objects = GetEquipment();
                int i = 0;
                foreach (var ob in objects)
                {
                    if (ob == item)
                    {
                        _SetEquipment((Slot)i, null);
                        break;
                    }
                    i++;
                }
            }
            _SetEquipment(slot, item);
        }

        private void _SetEquipment(Slot slot, Equipable item)
        {
            switch (slot)
            {
                case Slot.LEFT_HAND_WEAPON: leftHandWeaponObject = item; break;
                case Slot.RIGHT_HAND_WEAPON: rightHandWeaponObject = item; break;
                case Slot.GLOVES: glovesObject = item; break;
                case Slot.CHEST: chestObject = item; break;
                case Slot.HEAD: headObject = item; break;
                case Slot.LEGS: legsObject = item; break;
                case Slot.FEET: feetObject = item; break;
                case Slot.TWO_HANDED: twoHandedObject = item; break;
                case Slot.LEFT_HAND: leftHandObject = item; break;
                case Slot.RIGHT_HAND: rightHandObject = item; break;
            }

            onChange?.Invoke();
        }
    }
}
