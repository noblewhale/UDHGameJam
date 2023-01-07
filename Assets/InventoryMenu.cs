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
        public Light2D globalLightOff;
        public Light2D globalLightOn;

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
            if (globalLightOff)
            {
                globalLightOff.enabled = false;
            }
            if (globalLightOn)
            {
                globalLightOn.enabled = true;
            }
            var player = Player.instance.identity;
            player.transform.parent = characterPosition;
            player.transform.localPosition = new Vector3(-.5f, -.5f, 0);
            player.transform.localScale = Vector3.one;
            var lights = player.GetComponentsInChildren<Light2D>();
            foreach (var light in lights) light.enabled = false;
            foreach (Transform trans in player.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = this.gameObject.layer;
            }
        }

        public void OnDisable()
        {
            if (Player.instance && Player.instance.identity)
            {
                if (globalLightOff)
                {
                    globalLightOff.enabled = true;
                }
                if (globalLightOn)
                {
                    globalLightOn.enabled = false;
                }
                var player = Player.instance.identity;
                Tile tile = Map.instance.GetTile(player.x, player.y);
                tile.AddObject(player, false, 2);
                var lights = player.GetComponentsInChildren<Light2D>();
                foreach (var light in lights) light.enabled = true;
                foreach (Transform trans in player.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = Map.instance.gameObject.layer;
                }
            }
        }

        public void OnSelect(BaseEventData data)
        {
            //ReturnToDefaultMode();
        }

        public void EnableSlotsThatAllowItem(Equipable item)
        {
            if (item == null) return;
            var equipSlots = FindObjectsOfType<EquipSlotGUI>();
            foreach (var equipSlot in equipSlots)
            {
                if (item.allowedSlots.Any(s => equipSlot.slots.Contains(s)))
                {
                    //equipSlot.GetComponent<Image>().color = Color.green;
                    equipSlot.GetComponent<Button>().interactable = true;
                }
            }
        }

        public void DisableItemsThatDontFitSlots(Equipment.Slot[] slots)
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

        public void DisableOtherItems(Equipable item)
        {
            var inventoryGUIItems = FindObjectsOfType<InventorySlotGUI>();
            foreach (var inventoryGUIItem in inventoryGUIItems)
            {
                if (inventoryGUIItem.item.GetComponent<Equipable>() != item)
                {
                    inventoryGUIItem.GetComponent<Button>().interactable = false;
                }
            }
        }

        public void EnterAssignItemToSlotMode(Equipable equipment, bool autoSelectLikelySlot)
        {
            mode = Mode.ASSIGN_ITEM_TO_SLOT;
            currentItemForAssignment = equipment;
            EnableSlotsThatAllowItem(equipment);
            DisableOtherItems(equipment);
            if (autoSelectLikelySlot)
            {
                var equipSlotGUIS = FindObjectsOfType<EquipSlotGUI>();
                var mostLikelySlot = equipSlotGUIS.First(guiSlot => guiSlot.slots.Contains(equipment.allowedSlots[0]));
                mostLikelySlot.GetComponent<Button>().Select();
            }
        }

        public void EnterAssignSlotToItemMode(EquipSlotGUI equipSlotGUI, Equipable equipable)
        {
            mode = Mode.ASSIGN_SLOT_TO_ITEM;
            //var equipSlotGUIs = FindObjectsOfType<EquipSlotGUI>();
            //foreach (var equipSlot in equipSlotGUIs)
            //{
            //    var colorBlock = equipSlot.GetComponent<Button>().colors;
            //    colorBlock.selectedColor = Color.green;
            //    equipSlot.GetComponent<Button>().colors = colorBlock;
            //}
            currentSlotForAssignment = equipSlotGUI;
            currentItemForAssignment = equipable;
            DisableItemsThatDontFitSlots(equipSlotGUI.slots);
            //foreach (var slot in equipSlotGUI.slots)
            //{
            //    var equippedItem = Player.instance.identity.Creature.GetEquipment(slot);
            //    if (equippedItem)
            //    {
            //        EnableSlotsThatAllowItem(equippedItem);
            //    }
            //}
            var inventorySlots = FindObjectsOfType<InventorySlotGUI>();
            inventorySlots.Last(s => s.GetComponent<Button>().interactable).GetComponent<Button>().Select();

            equipSlotGUI.GetComponent<Image>().color = Color.green;
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

            if (currentItemForAssignment)
            {
                var inventorySlots = FindObjectsOfType<InventorySlotGUI>();
                inventorySlots.First(s => s.item == currentItemForAssignment.DungeonObject).GetComponent<Button>().Select();
            }

            currentItemForAssignment = null;
            currentSlotForAssignment = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            ReturnToDefaultMode();
        }
    }
}