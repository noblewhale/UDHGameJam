namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.UI;

    public class InventoryMenu : MonoBehaviour, IDropHandler
    {
        public static InventoryMenu instance;

        public Transform characterPosition;

        public enum Mode
        {
            DEFAULT,
            ASSIGN_ITEM_TO_SLOT,
            ASSIGN_SLOT_TO_ITEM
        }

        public Mode mode = Mode.DEFAULT;

        public EquipSlotGUI currentSlotForAssignment;
        public Equipable currentItemForAssignment;

        virtual protected void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            Player.instance.identity.transform.parent = characterPosition;
            Player.instance.identity.transform.localPosition = new Vector3(-.5f, -.5f, 0);
            Player.instance.identity.transform.localScale = Vector3.one;
            var lights = Player.instance.identity.GetComponentsInChildren<Light2D>();
            foreach (var light in lights) light.enabled = false;
            foreach (Transform trans in Player.instance.identity.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = this.gameObject.layer;
            }
        }

        public void OnSelect(BaseEventData data)
        {
            ReturnToDefaultMode();
        }

        void EnableSlotsThatAllowItem(Equipable item)
        {
            if (item == null) return;
            var equipSlots = FindObjectsOfType<EquipSlotGUI>();
            foreach (var equipSlot in equipSlots)
            {
                if (item.allowedSlots.Any(s => equipSlot.slots.Contains(s)))
                {
                    equipSlot.GetComponent<Image>().color = Color.green;
                    equipSlot.GetComponent<Button>().interactable = true;
                }
            }
        }

        void DisableItemsThatDontFitSlots(Equipment.Slot[] slots)
        {
            var inventoryGUIItems = FindObjectsOfType<InventorySlotGUI>();
            foreach (var inventoryGUIItem in inventoryGUIItems)
            {
                if (!inventoryGUIItem.item.GetComponent<Equipable>() || !inventoryGUIItem.item.GetComponent<Equipable>().allowedSlots.Any(s => slots.Contains(s)))
                {
                    inventoryGUIItem.GetComponent<Button>().interactable = false;
                }
            }
        }

        public void EnterAssignItemToSlotMode(Equipable equipment)
        {
            mode = Mode.ASSIGN_ITEM_TO_SLOT;
            currentItemForAssignment = equipment;
            EnableSlotsThatAllowItem(equipment);
        }

        public void EnterAssignSlotToItemMode(EquipSlotGUI equipSlotGUI, Equipable equipable)
        {
            mode = Mode.ASSIGN_SLOT_TO_ITEM;
            currentSlotForAssignment = equipSlotGUI;
            currentItemForAssignment = equipable;
            DisableItemsThatDontFitSlots(equipSlotGUI.slots);
            foreach (var slot in equipSlotGUI.slots)
            {
                var equippedItem = Player.instance.identity.Creature.GetEquipment(slot);
                if (equippedItem)
                {
                    EnableSlotsThatAllowItem(equippedItem);
                }
            }
        }

        public void ReturnToDefaultMode()
        {
            mode = Mode.DEFAULT;
            var equipSlots = FindObjectsOfType<EquipSlotGUI>();
            foreach (var equipSlot in equipSlots)
            {
                equipSlot.GetComponent<Image>().color = Color.white;
                equipSlot.GetComponent<Button>().interactable = false;
            }
            var inventoryGUISlots = FindObjectsOfType<InventorySlotGUI>();
            foreach (var inventoryGUISlot in inventoryGUISlots)
            {
                inventoryGUISlot.GetComponent<Button>().interactable = true;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            ReturnToDefaultMode();
        }
    }
}