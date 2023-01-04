namespace Noble.TileEngine
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Equipment : MonoBehaviour
    {

        public Equipable leftHandWeaponObject;
        public Equipable rightHandWeaponObject;
        public Equipable leftGloveObject;
        public Equipable rightGloveObject;
        public Equipable chestObject;
        public Equipable headObject;
        public Equipable legsObject;
        public Equipable feetObject;
        public Transform rightHandWeaponSlot;
        public Transform leftHandWeaponSlot;
        public Transform leftGloveSlot;
        public Transform rightGloveSlot;
        public Transform chestSlot;
        public Transform headSlot;
        public Transform legsSlot;
        public Transform feetSlot;

        public enum Slot
        {
            LEFT_HAND_WEAPON,
            RIGHT_HAND_WEAPON,
            LEFT_GLOVE,
            RIGHT_GLOVE,
            CHEST,
            HEAD,
            LEGS,
            FEET
        }

        IEnumerable<Equipable> GetEquipment()
        {
            yield return leftHandWeaponObject;
            yield return rightHandWeaponObject;
            yield return leftGloveObject;
            yield return rightGloveObject;
            yield return chestObject;
            yield return headObject;
            yield return legsObject;
            yield return feetObject;
        }

        public Transform GetSlotTransform(Slot slot)
        {
            switch (slot)
            {
                case Slot.LEFT_HAND_WEAPON: return leftHandWeaponSlot;
                case Slot.RIGHT_HAND_WEAPON: return rightHandWeaponSlot;
                case Slot.LEFT_GLOVE: return leftGloveSlot;
                case Slot.RIGHT_GLOVE: return rightGloveSlot;
                case Slot.CHEST: return chestSlot;
                case Slot.HEAD: return headSlot;
                case Slot.LEGS: return legsSlot;
                case Slot.FEET: return feetSlot;
                default: return null;
            }
        }

        public Equipable GetEquipment(Slot slot)
        {
            switch (slot)
            {
                case Slot.LEFT_HAND_WEAPON: return leftHandWeaponObject;
                case Slot.RIGHT_HAND_WEAPON: return rightHandWeaponObject;
                case Slot.LEFT_GLOVE: return leftGloveObject;
                case Slot.RIGHT_GLOVE: return rightGloveObject;
                case Slot.CHEST: return chestObject;
                case Slot.HEAD: return headObject;
                case Slot.LEGS: return legsObject;
                case Slot.FEET: return feetObject;
                default: return null;
            }
        }

        public void SetEquipment(Slot slot, Equipable item)
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
            _SetEquipment(slot, item);
        }

        private void _SetEquipment(Slot slot, Equipable item)
        {
            switch (slot)
            {
                case Slot.LEFT_HAND_WEAPON: leftHandWeaponObject = item; break;
                case Slot.RIGHT_HAND_WEAPON: rightHandWeaponObject = item; break;
                case Slot.LEFT_GLOVE: leftGloveObject = item; break;
                case Slot.RIGHT_GLOVE: rightGloveObject = item; break;
                case Slot.CHEST: chestObject = item; break;
                case Slot.HEAD: headObject = item; break;
                case Slot.LEGS: legsObject = item; break;
                case Slot.FEET: feetObject = item; break;
            }
        }
    }
}
