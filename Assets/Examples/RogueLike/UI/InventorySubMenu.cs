namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.UI;

    public class InventorySubMenu : MonoBehaviour
    {
        public RectTransform subMenu;
        public Button useButton;
        public Button equipButton;
        public Button equipLeftButton;
        public Button equipRightButton;
        public Button dropButton;

        DungeonObject item;

        public void OpenMenu(Vector2 mousePos, DungeonObject item)
        {
            this.item = item;
            gameObject.SetActive(true);

            Vector2 menuRectSize = InventoryMenu.instance.GetComponent<RectTransform>().rect.size;
            Vector2 mouseLocationAsPercentOfScreen = mousePos / new Vector2(Screen.width, Screen.height);
            subMenu.anchoredPosition = menuRectSize * mouseLocationAsPercentOfScreen;

            if (item.GetComponent<Equipable>())
            {
                InventoryMenu.instance.inventorySubMenu.ShowEquipableButtons(item.GetComponent<Equipable>());
            }
            else
            {
                InventoryMenu.instance.inventorySubMenu.ShowUsableItemButtons();
            }
        }
        public void CloseMenu()
        {
            gameObject.SetActive(false);
        }

        void ShowUsableItemButtons()
        {
            useButton.gameObject.SetActive(true);
            equipButton.gameObject.SetActive(false);
            equipLeftButton.gameObject.SetActive(false);
            equipRightButton.gameObject.SetActive(false);
            dropButton.gameObject.SetActive(true);
        }

        void ShowEquipableButtons(Equipable item)
        {
            useButton.gameObject.SetActive(false);
            if (item.allowedSlots.Contains(Equipment.Slot.LEFT_HAND_WEAPON) && item.allowedSlots.Contains(Equipment.Slot.RIGHT_HAND_WEAPON))
            {
                equipLeftButton.gameObject.SetActive(true);
                equipRightButton.gameObject.SetActive(true);
                equipButton.gameObject.SetActive(false);
            }
            else
            {
                equipLeftButton.gameObject.SetActive(false);
                equipRightButton.gameObject.SetActive(false);
                equipButton.gameObject.SetActive(true);
            }
            dropButton.gameObject.SetActive(true);
        }

        void ShowUnusableItemButtons()
        {
            useButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
            equipLeftButton.gameObject.SetActive(false);
            equipRightButton.gameObject.SetActive(false);
            dropButton.gameObject.SetActive(true);
        }

        public void OnEquipButtonPressed()
        {
            item.Equipable.Equip(Player.Identity.Creature, item.Equipable.allowedSlots[0]);
            EquipSlotGUI.UpdateAllSlots();
            CloseMenu();
        }

        public void OnEquipLeftButtonPressed()
        {
            item.Equipable.Equip(Player.Identity.Creature, Equipment.Slot.LEFT_HAND_WEAPON);
            EquipSlotGUI.UpdateAllSlots();
            CloseMenu();
        }

        public void OnEquipRightButtonPressed()
        {
            item.Equipable.Equip(Player.Identity.Creature, Equipment.Slot.RIGHT_HAND_WEAPON);
            EquipSlotGUI.UpdateAllSlots();
            CloseMenu();
        }
    }
}