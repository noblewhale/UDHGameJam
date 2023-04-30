namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using InventoryMode = InventoryMenu.Mode;

    [RequireComponent(typeof(DraggableItem))]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class InventorySlotGUI : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Transform glyphParent;
        public int index;
        public DungeonObject item;

        public List<Color> originalGlyphColors = new();

        Button button;
        public Selectable selectable;
        Image[] icons;
        InventoryMenu inventoryMenu => InventoryMenu.instance;

        public void Init(DungeonObject item)
        {
            this.item = item;
            button = GetComponent<Button>();
            selectable = GetComponent<Selectable>();
            GetComponent<DraggableItem>().Init(this.item.gameObject);
            transform.localRotation = Quaternion.identity;
            label.text = item.objectName;
            if (item.guiIcon)
            {
                foreach (var iconPart in item.guiIcon.GetComponentsInChildren<RectTransform>())
                {
                    var imageOb = Instantiate(iconPart.gameObject);
                    var imageRect = imageOb.GetComponentInParent<RectTransform>();
                    imageRect.SetParent(glyphParent, false);
                    imageOb.layer = gameObject.layer;
                }
                icons = glyphParent.GetComponentsInChildren<Image>();
                foreach (var icon in icons)
                {
                    originalGlyphColors.Add(icon.color);
                }
            }
        }

        public void Update()
        {
            if (button.interactable)
            {
                SetTint(1);
            }
            else if (item.Equipable == null || inventoryMenu.currentItemForAssignment != item.Equipable)
            {
                SetTint(.5f);
            }
        }

        public void SetTint(float amount)
        {
            label.color = Color.white * amount;
            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].color = originalGlyphColors[i] * amount;
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
                inventoryMenu.EnterAssignItemToSlotMode(item.Equipable);
            }
        }

        public void OnSelect(BaseEventData data)
        {
            if (!selectable.interactable)
            {
                if (data is AxisEventData axisEvent)
                {
                    EventSystemExtensions.SelectFirstInteractableInDirection(selectable, axisEvent);
                }
            }
            else
            {
                InventoryGUI.instance.lastSelectedSlot = this;
                var nav = InventoryGUI.instance.thingToConnectNavigationTo.navigation;
                nav.selectOnRight = selectable;
                InventoryGUI.instance.thingToConnectNavigationTo.navigation = nav;

                if (inventoryMenu.mode == InventoryMode.ASSIGN_SLOT_TO_ITEM)
                {
                    if (item.Equipable)
                    {
                        if (item.Equipable.useOverrideSlot)
                        {
                            foreach (var equipSlot in EquipSlotGUI.AllSlots)
                            {
                                if (equipSlot.slots.Contains(item.Equipable.overrideSlot))
                                {
                                    equipSlot.image.color = Color.green;
                                    equipSlot.button.interactable = true;
                                }
                            }
                        }
                        //else
                        //{
                        //    InventoryMenu.instance.currentSlotForAssignment.EnableAndSelect();
                        //}
                    }
                }
            }
        }

        public void OnDeselect(BaseEventData data)
        {
            if (inventoryMenu.mode == InventoryMode.ASSIGN_SLOT_TO_ITEM)
            {
                if (item.Equipable && item.Equipable.useOverrideSlot)
                {
                    foreach (var equipSlot in EquipSlotGUI.AllSlots)
                    {
                        if (inventoryMenu.fakeSelectedSlots.Contains(equipSlot)) continue;

                        equipSlot.image.color = Color.white;
                        equipSlot.button.interactable = false;
                    }
                }
            }
        }

        public void OnSubmit(BaseEventData data)
        {
            switch (inventoryMenu.mode)
            {
                case InventoryMode.DEFAULT:
                    inventoryMenu.EnterAssignItemToSlotMode(item.Equipable);
                    break;
                case InventoryMode.ASSIGN_SLOT_TO_ITEM:
                    inventoryMenu.currentSlotForAssignment.EquipItem(item.Equipable);
                    inventoryMenu.ReturnToDefaultMode();
                    break;
            }
        }

        public void OnMousePressed(BaseEventData data)
        {
            var pointerData = data as PointerEventData;
            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                OnSubmit(data);
            }
            else if (pointerData.button == PointerEventData.InputButton.Right)
            {
                InventoryMenu.instance.inventorySubMenu.gameObject.SetActive(true);

                InventoryMenu.instance.inventorySubMenu.OpenMenu(pointerData.position, item);
            }
        }

        public void OnMouseEnter(BaseEventData data)
        {
            if (inventoryMenu.mode == InventoryMode.ASSIGN_ITEM_TO_SLOT) return;
            button.Select();
        }
    }
}