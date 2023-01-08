namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using InventoryMode = InventoryMenu.Mode;

    [RequireComponent(typeof(DraggableItem))]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class EquipSlotGUI : MonoBehaviour, IDropHandler
    {
        public Equipment.Slot[] slots = new Equipment.Slot[0];
        DungeonObject content;

        DraggableItem draggedItem;
        Button button;
        Selectable selectable;
        Image image;
        Rect boundsRect;

        InventoryMenu inventoryMenu => InventoryMenu.instance;

        static EquipSlotGUI[] AllSlots;

        void Awake()
        {
            button = GetComponent<Button>();
            selectable = GetComponent<Selectable>();
            image = GetComponent<Image>();
            draggedItem = GetComponent<DraggableItem>();
            boundsRect = GetComponent<RectTransform>().rect;
        }

        void Start()
        {
            if (AllSlots == null)
            {
                AllSlots = FindObjectsOfType<EquipSlotGUI>();
            }
        }

        void OnDestroy()
        {
            AllSlots = null;
        }

        void OnEnable()
        {
            UpdateSlot();
        }

        void UpdateSlot()
        {
            if (!Player.Identity) return;
            if (content) Destroy(content.gameObject);
            foreach (var slot in slots)
            {
                var equippedItem = Player.Identity.Equipment.GetEquipment(slot);
                if (equippedItem != null)
                {
                    SpawnItemClone(equippedItem);
                    break;
                }
            }
        }

        void SpawnItemClone(Equipable equippedItem)
        {
            draggedItem.Init(equippedItem.gameObject);
            var contentOb = GameObject.Instantiate(equippedItem.gameObject);
            content = contentOb.GetComponent<DungeonObject>();

            var glyphsComponent = content.glyphs;
            glyphsComponent.gameObject.SetActive(true);
            glyphsComponent.SetLit(true);
            glyphsComponent.ResetGlyphColors();

            foreach (Transform trans in content.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = gameObject.layer;
            }

            content.transform.position = Vector3.zero;
            Bounds combinedBounds = content.gameObject.GetCombinedBounds();

            Vector2 dim = combinedBounds.size;
            float scale;
            if (dim.x > dim.y)
            {
                scale = boundsRect.width / dim.x;
            }
            else
            {
                scale = boundsRect.height / dim.y;
            }

            scale *= .8f;

            content.transform.parent = transform;
            content.transform.localPosition = -combinedBounds.center * scale;
            content.transform.localScale = new Vector3(scale, scale, 1);
            content.transform.localPosition = new Vector3(content.transform.localPosition.x, content.transform.localPosition.y, -.1f);
            content.transform.localPosition += (Vector3)boundsRect.size / 2;
        }

        public void EquipItem(Equipable item)
        {
            if (item == null) return;
            if (!item.allowedSlots.Any(s => slots.Contains(s))) return;

            item.Equip(Player.Creature, slots[0]);
            UpdateAllSlots();
        }

        static void UpdateAllSlots()
        {
            foreach (var equipSlot in AllSlots)
            {
                equipSlot.UpdateSlot();
            }
        }

        public void UnEquipSlot()
        {
            foreach (var slot in slots)
            {
                var equippedItem = Player.Equipment.GetEquipment(slot);
                if (equippedItem != null)
                {
                    var itemToUnequip = equippedItem;
                    if (itemToUnequip.parentItem != null) itemToUnequip = itemToUnequip.parentItem;
                    itemToUnequip.UnEquip();
                    break;
                }
            }

            UpdateAllSlots();
        }

        void AssignItemAndReturnToDefault()
        {
            EquipItem(inventoryMenu.currentItemForAssignment);
            inventoryMenu.ReturnToDefaultMode();
            EnableAndSelect();
        }

        void AssignSlotAndReturnToDefault()
        {
            if (inventoryMenu.currentSlotForAssignment != this && inventoryMenu.currentItemForAssignment != null)
            { 
                // Swap items
                var temp = Player.Equipment.GetEquipment(slots[0]);
                EquipItem(inventoryMenu.currentItemForAssignment);
                if (temp)
                {
                    inventoryMenu.currentSlotForAssignment.EquipItem(temp);
                }
                inventoryMenu.ReturnToDefaultMode();
            }
        }

        public void OnCancel(BaseEventData data)
        {
            if (inventoryMenu.mode == InventoryMode.DEFAULT) UnEquipSlot();
        }

        public void OnSelect(BaseEventData data)
        {
            switch (inventoryMenu.mode)
            {
                case InventoryMode.DEFAULT: button.interactable = true; break;
                case InventoryMode.ASSIGN_ITEM_TO_SLOT: Select(data);  break;
            }
        }

        public void OnDeselect(BaseEventData data)
        {
            switch (inventoryMenu.mode)
            {
                case InventoryMode.DEFAULT: StartCoroutine(DeactivateAtEndOfFrame()); break;
                case InventoryMode.ASSIGN_ITEM_TO_SLOT: Unselect(); break;
            }
        }

        void Select(BaseEventData data)
        {
            if (button.interactable)
            {
                image.color = Color.green;
            }
            else if (data is AxisEventData axisEvent)
            {
                EventSystemExtensions.SelectFirstInteractableInDirection(selectable, axisEvent);
            }
        }

        void Unselect()
        {
            if (!button.interactable) return;
            if (inventoryMenu.fakeSelectedSlots.Contains(this)) return;
            image.color = Color.white;
        }

        IEnumerator DeactivateAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            button.interactable = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!button.interactable) return;
            switch (inventoryMenu.mode)
            {
                case InventoryMode.ASSIGN_ITEM_TO_SLOT: AssignItemAndReturnToDefault(); break;
                case InventoryMode.ASSIGN_SLOT_TO_ITEM: AssignSlotAndReturnToDefault(); break;
            }
        }

        public void OnSubmit(BaseEventData data)
        {
            var item = Player.Equipment.GetEquipment(slots[0]);
            switch (inventoryMenu.mode)
            {
                case InventoryMode.DEFAULT: inventoryMenu.EnterAssignSlotToItemMode(this, item); break;
                case InventoryMode.ASSIGN_ITEM_TO_SLOT: AssignItemAndReturnToDefault(); break;
                case InventoryMode.ASSIGN_SLOT_TO_ITEM: AssignSlotAndReturnToDefault(); break;
            }
        }

        public void OnBeginDrag(BaseEventData data)
        {
            if (data is PointerEventData pointerData)
            {
                if (pointerData.button != PointerEventData.InputButton.Left)
                {
                    return;
                }
            }
            if (inventoryMenu.mode == InventoryMode.DEFAULT)
            {
                inventoryMenu.EnterAssignSlotToItemMode(this, Player.Equipment.GetEquipment(slots[0]));
            }
        }

        public void OnMousePressed(BaseEventData data)
        {
            if (data is PointerEventData pointerData)
            {
                if (pointerData.button == PointerEventData.InputButton.Left)
                {
                    OnSubmit(data);
                }
                else if (pointerData.button == PointerEventData.InputButton.Right)
                {
                    OnCancel(data);
                }
            }
        }

        public void OnMouseEnter(BaseEventData data)
        {
            switch (inventoryMenu.mode)
            {
                case InventoryMode.DEFAULT: EnableAndSelect(); break;
                case InventoryMode.ASSIGN_ITEM_TO_SLOT: if (button.interactable) button.Select(); break;
            }
        }

        public void EnableAndSelect()
        {
            button.interactable = true;
            button.Select();
        }

    }
}