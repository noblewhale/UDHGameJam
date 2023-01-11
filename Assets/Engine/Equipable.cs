namespace Noble.TileEngine
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    [RequireComponent(typeof(DungeonObject))]
    public class Equipable : MonoBehaviour
    {
        public bool IsEquipped { get; private set; }
        public Creature EquippedBy { get; private set; }

        // The handle to line up with the slot transform
        // This determines the position of the item relative to the equipment slot on the creature
        public Transform handle;

        // Determines which slots an item is allowed to be equipped to
        // These slots will be highlighted when the item is selected for equipping
        public Equipment.Slot[] allowedSlots = new Equipment.Slot[0];

        // Used for two-handed weapons that can be assigned to left or right hand, but actually get assigned to the TWO_HANDED slot
        public bool useOverrideSlot = false;
        public Equipment.Slot overrideSlot = Equipment.Slot.TWO_HANDED;

        // Used for two-handed weapons that are assigned to the TWO_HANDED slot but additionally occupy the left and right hand weapon slots.
        // When this item is equipped, the extra slots will be automatically unequiped
        public Equipment.Slot[] occupyExtraSlots = new Equipment.Slot[0];

        // Should this item equip automatically when picked up
        public bool autoEquip = false;
        public Equipment.Slot autoEquipSlot;

        // This class is used because Unity doesn't support dictionaries in the inspector
        // so instead we use an array of these SlotObjects that gets converted to a 
        // dictionary at run time
        [Serializable]
        public class SlotObject
        {
            public GameObject ob;
            public Equipment.Slot slot;
        }

        // Used by weapons like the spear that look different depending on what hand they are in
        public SlotObject[] objectsToEnableForSlot;
        // This is the dictionary we actually use for quick look up by slot
        Dictionary<Equipment.Slot, GameObject> _objectsToEnableForSlot;

        // The slot this equipable is assigned to (only valid when IsEquipped)
        public Equipment.Slot assignedSlot;

        // Used by items like the gloves that are actually made of sub-items that go in separate slots
        List<Equipable> subItems = new();
        public Equipable parentItem;

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
            // Convert array of SlotObjects to more useful dictionary
            _objectsToEnableForSlot = new Dictionary<Equipment.Slot, GameObject>();
            foreach (var slotObject in objectsToEnableForSlot)
            {
                _objectsToEnableForSlot.Add(slotObject.slot, slotObject.ob);
            }
            DungeonObject.onPickedUp.AddListener(OnPickedUp);
        }

        public void Start()
        {
            if (transform.parent)
            {
                parentItem = transform.parent.GetComponentInParent<Equipable>(true);
            }
            subItems = new List<Equipable>(GetComponentsInChildren<Equipable>(true));
            subItems = subItems.Where(item => item.gameObject != gameObject).ToList();
        }

        public void OnDestroy()
        {
            DungeonObject.onPickedUp.RemoveListener(OnPickedUp);
        }

        void OnPickedUp(DungeonObject _, DungeonObject equipper)
        {
            if (!autoEquip) return;

            var equipperAsCreature = equipper.Creature;
            if (!equipperAsCreature) return;
            
            Equip(equipperAsCreature, autoEquipSlot);
        }

        public void Equip(Creature equipper, Equipment.Slot slot)
        {
            var equipment = equipper.Equipment;
            if (!equipment) return;

            assignedSlot = slot;

            if (useOverrideSlot)
            {
                assignedSlot = overrideSlot;
            }

            equipment.GetEquipment(assignedSlot)?.UnEquip();

            foreach (var objectSlot in _objectsToEnableForSlot)
            {
                objectSlot.Value.SetActive(false);
            }
            if (_objectsToEnableForSlot.ContainsKey(slot))
            {
                _objectsToEnableForSlot[slot].SetActive(true);
            }

            // Add sub-items to their respective slots
            foreach (var subItem in subItems)
            {
                subItem.gameObject.SetActive(true);
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

            IsEquipped = true;
            EquippedBy = equipper;

            UpdatePosition();

            if (DungeonObject && DungeonObject.glyphs)
            {
                DungeonObject.glyphs.SetLit(true);
            }

            equipment.SetEquipment(assignedSlot, this);
        }

        public void UpdatePosition()
        {
            var slotTransform = EquippedBy.Equipment.GetSlotTransform(assignedSlot);
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
            EquippedBy.Equipment.SetEquipment(assignedSlot, null);
            
            // Remove subitems from slot and add them back to parent
            foreach (var subItem in subItems)
            {
                subItem.UnEquip();
                subItem.transform.parent = transform;
                subItem.gameObject.SetActive(false);
            }

            IsEquipped = false;
            EquippedBy = null;
            transform.parent = null;
            transform.position = new Vector3(-666, -666, -666);
        }
    }
}