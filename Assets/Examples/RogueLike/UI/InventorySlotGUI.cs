namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine.UIElements;
    using Button = UnityEngine.UI.Button;
    using Image = UnityEngine.UI.Image;

    [RequireComponent(typeof(DraggableItem))]
    public class InventorySlotGUI : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Transform glyphParent;
        public int index;
        public DungeonObject item;

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
            }
        }

        public void Update()
        {
            Weapon weapon = item.GetComponent<Weapon>();
            if (weapon != null && weapon.Weildable.IsEquipped)
            {
                label.text = "[" + item.objectName + "]";
            }
            else
            {
                label.text = item.objectName;
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

        public void OnDeSelected(BaseEventData data)
        {
            //InventoryGUI.instance.mode = InventoryGUI.Mode.DEFAULT;
            //var equipSlots = FindObjectsOfType<EquipSlotGUI>();
            //var equipment = item.GetComponent<Equipable>();
            //foreach (var equipSlot in equipSlots)
            //{
            //    if (equipment.allowedSlots.Contains(equipSlot.slot))
            //    {
            //        equipSlot.GetComponent<Image>().color = Color.white;
            //        equipSlot.GetComponent<Button>().interactable = false;
            //    }
            //}
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
                        if (equipment.allowedSlots.Contains(equipSlot.slot))
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