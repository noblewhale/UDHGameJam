namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.UI;
    using UnityEngine.InputSystem;
    using System.Collections.Generic;

    public class InventoryMenu : MonoBehaviour, IDropHandler
    {
        public static InventoryMenu instance;

        public Transform characterPosition;
        public Light2D globalLightOff;
        public Light2D globalLightOn;

        public InventoryGUI inventoryGUI;

        public enum Mode
        {
            DEFAULT,
            ASSIGN_ITEM_TO_SLOT,
            ASSIGN_SLOT_TO_ITEM
        }

        public Mode mode = Mode.DEFAULT;

        public EquipSlotGUI currentSlotForAssignment;
        public Equipable currentItemForAssignment;
        public List<EquipSlotGUI> fakeSelectedSlots = new();

        virtual protected void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
            inventoryGUI = GetComponentInChildren<InventoryGUI>();
        }

        public void OnEnable()
        {
            Cursor.visible = true;
            
            var player = Player.Identity;
            player.transform.parent = characterPosition;
            player.transform.localPosition = new Vector3(-.5f, -.5f, 0);
            player.transform.localScale = Vector3.one;

            SetupLightsAndLayers(false, gameObject.layer);
        }

        public void OnDisable()
        {
            if (Player.Identity)
            {
                var player = Player.Identity;
                Tile tile = Map.instance.GetTile(player.x, player.y);
                tile.AddObject(player, false, 2);

                SetupLightsAndLayers(true, Map.instance.gameObject.layer);
            }
        }

        void SetupLightsAndLayers(bool lightingOn, int layer)
        {
            if (globalLightOff) globalLightOff.enabled = lightingOn;
            if (globalLightOn) globalLightOn.enabled = !lightingOn;

            var lights = Player.Identity.GetComponentsInChildren<Light2D>();
            foreach (var light in lights) light.enabled = lightingOn;

            var transforms = Player.Identity.GetComponentsInChildren<Transform>(true);
            foreach (var trans in transforms) trans.gameObject.layer = layer;
        }

        public void Update()
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                CloseMenu();
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (mode == Mode.DEFAULT)
                {
                    CloseMenu();
                }
                else
                {
                    ReturnToDefaultMode();
                }
            }
        }

        public void CloseMenu()
        {
            ReturnToDefaultMode();
            gameObject.SetActive(false);
            PlayerInputHandler.instance.enabled = true;
        }

        public void OnMouseClick(BaseEventData data)
        {
            if (mode != Mode.DEFAULT)
            {
                ReturnToDefaultMode();
            }
            else
            {
                EventSystemExtensions.LastSelectedGameObject.GetComponent<Button>().Select();
            }
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
                //if (inventoryGUIItem.item.GetComponent<Equipable>() != item)
                {
                    inventoryGUIItem.GetComponent<Button>().interactable = false;
                }
            }
        }

        public void EnterAssignItemToSlotMode(Equipable equipment)
        {
            if (equipment == null) return;

            mode = Mode.ASSIGN_ITEM_TO_SLOT;
            currentItemForAssignment = equipment;
            EnableSlotsThatAllowItem(equipment);
            DisableOtherItems(equipment);
            var nav = inventoryGUI.thingToConnectNavigationTo.navigation;
            nav.selectOnRight = null;
            inventoryGUI.thingToConnectNavigationTo.navigation = nav;

            var equipSlotGUIS = FindObjectsOfType<EquipSlotGUI>();
            if (equipment.useOverrideSlot)
            {
                var mostLikelySlots = equipSlotGUIS.Where(guiSlot => guiSlot.slots.Contains(equipment.overrideSlot));
                fakeSelectedSlots.Clear();
                mostLikelySlots.First().GetComponent<Button>().Select();
                foreach (var slot in mostLikelySlots)
                {
                    fakeSelectedSlots.Add(slot);
                    if (slot == mostLikelySlots.First()) continue;
                    slot.GetComponent<Image>().color = Color.green;
                }
            }
            else
            {
                var mostLikelySlot = equipSlotGUIS.First(guiSlot => guiSlot.slots.Contains(equipment.allowedSlots[0]));
                mostLikelySlot.GetComponent<Button>().Select();
            }
        }

        public void EnterAssignSlotToItemMode(EquipSlotGUI equipSlotGUI, Equipable equipable)
        {
            mode = Mode.ASSIGN_SLOT_TO_ITEM;
            currentSlotForAssignment = equipSlotGUI;
            currentItemForAssignment = equipable;
            DisableItemsThatDontFitSlots(equipSlotGUI.slots);
            var inventorySlots = FindObjectsOfType<InventorySlotGUI>();
            inventorySlots.Last(s => s.GetComponent<Button>().interactable).GetComponent<Button>().Select();

            foreach (var inventorySlot in inventorySlots)
            {
                var nav = inventorySlot.GetComponent<Selectable>().navigation;
                nav.selectOnLeft = null;
                inventorySlot.GetComponent<Selectable>().navigation = nav;
            }

            equipSlotGUI.GetComponent<Image>().color = Color.green;
        }

        public void ReturnToDefaultMode()
        {
            var inventorySlots = inventoryGUI.slots.Values;

            if (mode == Mode.ASSIGN_ITEM_TO_SLOT)
            {
                if (currentItemForAssignment)
                {
                    inventorySlots.First(s => s.item == currentItemForAssignment.DungeonObject).GetComponent<Button>().Select();
                }
            }

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
            
            var nav = inventoryGUI.thingToConnectNavigationTo.navigation;
            nav.selectOnRight = inventoryGUI.lastSelectedSlot.GetComponent<Selectable>();
            inventoryGUI.thingToConnectNavigationTo.navigation = nav;

            foreach (var inventorySlot in inventorySlots)
            {
                nav = inventorySlot.GetComponent<Selectable>().navigation;
                nav.selectOnLeft = inventoryGUI.thingToConnectNavigationTo;
                inventorySlot.GetComponent<Selectable>().navigation = nav;
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