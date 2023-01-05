namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(DraggableItem))]
    public class EquipSlotGUI : MonoBehaviour, IDropHandler
    {
        public Equipment.Slot[] slots = new Equipment.Slot[0];
        public GameObject content;

        void OnEnable()
        {
            UpdateSlot();
        }

        void UpdateSlot()
        {
            if (!Player.instance || !Player.instance.identity) return;
            if (content) Destroy(content);
            var playerCreature = Player.instance.identity.GetComponent<Creature>();
            foreach (var slot in slots)
            {
                var equippedItem = playerCreature.GetComponent<Equipment>().GetEquipment(slot);
                if (equippedItem != null)
                {
                    GetComponent<DraggableItem>().itemToDrag = equippedItem.gameObject;
                    content = GameObject.Instantiate(equippedItem.gameObject);
                    
                    var glyphsComponent = content.GetComponentInChildren<Glyphs>(true);
                    glyphsComponent.GetComponentInChildren<Glyphs>(true).enabled = false;
                    glyphsComponent.gameObject.SetActive(true);
                    glyphsComponent.SetLit(true);
                    foreach (var glyph in glyphsComponent.glyphs)
                    {
                        glyph.GetComponent<SpriteRenderer>().color = glyph.originalColor;
                    }
                    foreach (Transform trans in content.GetComponentsInChildren<Transform>(true))
                    {
                        trans.gameObject.layer = this.gameObject.layer;
                    }
                    content.transform.parent = transform;
                    var rect = transform.GetComponent<RectTransform>().rect;
                    content.transform.localScale = new Vector3(rect.width, rect.height, 1);
                    content.transform.localPosition = Vector3.zero;
                    content.transform.localPosition -= Vector3.forward * .1f;
                    break;
                }
            }
        }

        public void EquipItem(GameObject item)
        {
            if (item == null) return;
            if (!item.GetComponent<Equipable>()) return;
            if (!item.GetComponent<Equipable>().allowedSlots.Any(s => slots.Contains(s))) return;

            item.GetComponent<Equipable>().Equip(Player.instance.identity.Creature, slots[0]);
            var allEquipSlots = FindObjectsOfType<EquipSlotGUI>();
            foreach (var equipSlot in allEquipSlots)
            {
                equipSlot.UpdateSlot();
            }
        }

        void AssignItemAndReturnToDefault()
        {
            EquipItem(InventoryMenu.instance.currentItemForAssignment.gameObject);
            InventoryMenu.instance.ReturnToDefaultMode();
            var inventorySlots = FindObjectsOfType<InventorySlotGUI>();
            inventorySlots.First(s => s.item == InventoryMenu.instance.currentItemForAssignment.DungeonObject).GetComponent<Button>().Select();
        }

        void AssignSlotAndReturnToDefault()
        {
            if (InventoryMenu.instance.currentSlotForAssignment != this && InventoryMenu.instance.currentItemForAssignment != null)
            { 
                // Swap items
                var temp = Player.instance.identity.Creature.GetEquipment(slots[0]);
                EquipItem(InventoryMenu.instance.currentItemForAssignment.gameObject);
                if (temp)
                {
                    InventoryMenu.instance.currentSlotForAssignment.EquipItem(temp.gameObject);
                }
                InventoryMenu.instance.ReturnToDefaultMode();
            }
        }

        public void OnSelect(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                GetComponent<Button>().interactable = true;
            }
            else
            {
                if (!GetComponent<Button>().interactable && data is AxisEventData axisEvent)
                {
                    var nextSelectable = GetComponent<Button>().FindSelectable(axisEvent.moveVector);
                    if (nextSelectable == null)
                    {
                        nextSelectable = GetComponent<Button>().FindSelectable(axisEvent.moveVector * -1);
                    }
                    StartCoroutine(DelaySelect(nextSelectable));
                    return;
                }
            }
        }

        public void OnDeselect(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                GetComponent<Button>().interactable = false;
            }
        }

        IEnumerator DelaySelect(Selectable nextSelectable)
        {
            yield return new WaitForEndOfFrame();
            nextSelectable.Select();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!GetComponent<Button>().interactable) return;
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.ASSIGN_SLOT_TO_ITEM)
            {
                AssignSlotAndReturnToDefault();
            }
            else if(InventoryMenu.instance.mode == InventoryMenu.Mode.ASSIGN_ITEM_TO_SLOT)
            {
                AssignItemAndReturnToDefault();
            }
        }

        public void OnMouseDown(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                InventoryMenu.instance.EnterAssignSlotToItemMode(this, Player.instance.identity.Creature.GetEquipment(slots[0]));
            }
        }

        public void OnMousePress(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.ASSIGN_ITEM_TO_SLOT)
            {
                AssignItemAndReturnToDefault();
            }
            else if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                InventoryMenu.instance.EnterAssignSlotToItemMode(this, Player.instance.identity.Creature.GetEquipment(slots[0]));
            }
            else if (InventoryMenu.instance.mode == InventoryMenu.Mode.ASSIGN_SLOT_TO_ITEM)
            {
                AssignSlotAndReturnToDefault();
            }
        }
        
        public void OnMouseEnter(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                GetComponent<Button>().interactable = true;
            }
        }

        public void OnMouseExit(BaseEventData data)
        {
            if (InventoryMenu.instance.mode == InventoryMenu.Mode.DEFAULT)
            {
                GetComponent<Button>().interactable = false;
            }
        }

    }
}