namespace Noble.TileEngine
{
    using System.Xml;
    using UnityEngine;
    using UnityEngine.Rendering;

    [RequireComponent(typeof(DungeonObject))]
    public class Equipable : MonoBehaviour
    {
        public bool IsEquipped { get; private set; }
        public Creature EquippedBy { get; private set; }

        public Transform handle;

        public Equipment.Slot[] allowedSlots = new Equipment.Slot[0];

        public bool autoEquip = false;
        public Equipment.Slot autoEquipSlot;

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

            IsEquipped = true;
            EquippedBy = equipper;

            var slotTransform = equipment.GetSlotTransform(slot);
            transform.parent = slotTransform;
            Vector3 handlePos = transform.position;
            if (handle)
            {
                handlePos = handle.position;
            }
            transform.position = slotTransform.position - (handlePos - transform.position);

            equipment.SetEquipment(slot, this);
        }

        public void UnEquip()
        {
            IsEquipped = false;
            EquippedBy = null;
            transform.parent = null;
            transform.position = new Vector3(-666, -666, -666);
        }
    }
}