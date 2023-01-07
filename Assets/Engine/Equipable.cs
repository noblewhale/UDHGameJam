namespace Noble.TileEngine
{
    using System.Collections.Specialized;
    using System.Xml;
    using UnityEngine;
    using System.Collections.Generic;
    using UnityEngine.Rendering;
    using System.Linq;
    using System;

    [RequireComponent(typeof(DungeonObject))]
    public class Equipable : MonoBehaviour
    {
        public bool IsEquipped { get; private set; }
        public Creature EquippedBy { get; private set; }

        public Transform handle;

        public Equipment.Slot[] allowedSlots = new Equipment.Slot[0];

        // Used for two-handed weapons
        public bool useOverrideSlot = false;
        public Equipment.Slot overrideSlot = Equipment.Slot.TWO_HANDED;

        public Equipment.Slot[] occupyExtraSlots = new Equipment.Slot[0];

        public bool autoEquip = false;
        public Equipment.Slot autoEquipSlot;

        [Serializable]
        public class SlotObject
        {
            public GameObject ob;
            public Equipment.Slot slot;
        }

        public SlotObject[] objectsToEnableForSlot;
        Dictionary<Equipment.Slot, GameObject> _objectsToEnableForSlot;

        public Equipment.Slot actualSlot;

        DungeonObject _object;
        public DungeonObject DungeonObject
        {
            get
            {
                if (_object == null) _object = GetComponent<DungeonObject>();
                return _object;
            }
        }

        public void Awake()
        {
            _objectsToEnableForSlot = new Dictionary<Equipment.Slot, GameObject>();
            foreach (var slotObject in objectsToEnableForSlot)
            {
                _objectsToEnableForSlot.Add(slotObject.slot, slotObject.ob);
            }
            DungeonObject.onPickedUp += OnPickedUp;
        }

        public void OnDestroy()
        {
            DungeonObject.onPickedUp -= OnPickedUp;
        }

        void OnPickedUp(DungeonObject equipper)
        {
            if (!autoEquip) return;

            var equipperAsCreature = equipper.GetComponent<Creature>();
            if (!equipperAsCreature) return;
            
            Equip(equipperAsCreature, autoEquipSlot);
        }

        public void Equip(Creature equipper, Equipment.Slot slot)
        {
            var equipment = equipper.GetComponent<Equipment>();
            if (!equipment) return;

            foreach (var objectSlot in _objectsToEnableForSlot)
            {
                objectSlot.Value.SetActive(false);
            }
            if (_objectsToEnableForSlot.ContainsKey(slot))
            {
                _objectsToEnableForSlot[slot].SetActive(true);
            }

            // Add sub-items to their respective slots
            var subEquipment = GetComponentsInChildren<Equipable>();
            foreach (var subItem in subEquipment)
            {
                if (subItem.transform == transform) continue;
                subItem.Equip(equipper, subItem.allowedSlots[0]);
            }

            // Remove items in extra slots that this item occupies
            // Used when assigning two handed weapons that are in the two-handed slot but additionally occupy the left and right hand slots
            foreach (var otherSlot in occupyExtraSlots)
            {
                equipment.GetEquipment(otherSlot)?.UnEquip();
            }

            // Remove any equipment that is occupying the slot being assigned to
            // Used when removing two handed weapons that are in the two-handed slot but additionally occupy the left and right hand slots
            var allEquipment = equipment.GetEquipment();
            foreach (var item in allEquipment)
            {
                if (item && item.occupyExtraSlots.Contains(slot))
                {
                    item.UnEquip();
                }
            }

            actualSlot = slot;

            if (useOverrideSlot)
            {
                actualSlot = overrideSlot;
            }

            equipment.GetEquipment(actualSlot)?.UnEquip();

            IsEquipped = true;
            EquippedBy = equipper;

            var slotTransform = equipment.GetSlotTransform(actualSlot);
            transform.parent = slotTransform;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            Vector3 handlePos = Vector3.zero;
            if (handle)
            {
                handlePos = handle.localPosition;
            }
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = equipper.gameObject.layer;
            }
            transform.localPosition = -handlePos;

            if (DungeonObject && DungeonObject.glyphs)
            {
                DungeonObject.glyphs.SetLit(true);
            }

            equipment.SetEquipment(actualSlot, this);
        }

        public void UpdatePosition()
        {
            var equipment = EquippedBy.GetComponent<Equipment>();
            var slotTransform = equipment.GetSlotTransform(actualSlot);
            transform.parent = slotTransform;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            Vector3 handlePos = Vector3.zero;
            if (handle)
            {
                handlePos = transform.InverseTransformPoint(handle.position);
            }
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = EquippedBy.gameObject.layer;
            }
            transform.localPosition = -handlePos;
        }

        public void UnEquip()
        {
            var equipment = EquippedBy.GetComponent<Equipment>();
            equipment.SetEquipment(actualSlot, null);
            IsEquipped = false;
            EquippedBy = null;
            transform.parent = null;
            transform.position = new Vector3(-666, -666, -666);
        }
    }
}