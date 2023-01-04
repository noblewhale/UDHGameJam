namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(DraggableItem))]
    public class InventorySlotGUI : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Transform glyphParent;
        public int index;
        public DungeonObject item;

        public List<Color> originalGlyphColors = new();

        public void Init(DungeonObject item)
        {
            this.item = item;
            GetComponent<DraggableItem>().itemToDrag = this.item.gameObject;
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
                var icons = glyphParent.GetComponentsInChildren<Image>();
                foreach (var icon in icons)
                {
                    originalGlyphColors.Add(icon.color);
                }
            }
        }

        public void Update()
        {
            if (GetComponent<Button>().interactable)
            {
                label.color = Color.white;
                var icons = glyphParent.GetComponentsInChildren<Image>();
                int i = 0;
                foreach (var icon in icons)
                {
                    icon.color = originalGlyphColors[i];
                    i++;
                }
            }
            else
            {
                label.color = Color.white / 2;
                var icons = glyphParent.GetComponentsInChildren<Image>();
                int i = 0;
                foreach (var icon in icons)
                {
                    icon.color = originalGlyphColors[i] / 2;
                    i++;
                }
            }
        }

        public void OnMouseDown(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT || InventoryMenu.instance.mode == InventoryMenu.Mode.ASSIGN_ITEM_TO_SLOT)
            {
                GetComponent<Button>().Select();
                var equipment = item.GetComponent<Equipable>();
                if (equipment)
                {
                    InventoryMenu.instance.EnterAssignItemToSlotMode(equipment);
                }
            }
        }

        public void OnMousePressed(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT || InventoryMenu.instance.mode == InventoryMenu.Mode.ASSIGN_ITEM_TO_SLOT)
            {
                var equipment = item.GetComponent<Equipable>();
                if (equipment)
                {
                    InventoryMenu.instance.EnterAssignItemToSlotMode(equipment);
                }
            }
            else if (InventoryMenu.instance.mode == InventoryMenu.Mode.ASSIGN_SLOT_TO_ITEM)
            {
                InventoryMenu.instance.currentSlotForAssignment.EquipItem(item.gameObject);
                InventoryMenu.instance.ReturnToDefaultMode();
            }
        }

        public void OnMouseEnter(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                var equipment = item.GetComponent<Equipable>();
                if (equipment)
                {
                    var equipSlots = FindObjectsOfType<EquipSlotGUI>();
                    foreach (var equipSlot in equipSlots)
                    {
                        if (equipment.allowedSlots.Any(s => equipSlot.slots.Contains(s)))
                        {
                            equipSlot.GetComponent<Button>().interactable = true;
                        }
                    }
                }
            }
        }

        public void OnMouseExit(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                var equipSlots = FindObjectsOfType<EquipSlotGUI>();
                foreach (var equipSlot in equipSlots)
                {
                    if (equipSlot.GetComponent<Image>().color != Color.green)
                    {
                        equipSlot.GetComponent<Button>().interactable = false;
                    }
                }
            }
        }
    }
}