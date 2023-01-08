namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class InventoryGUI : MonoBehaviour
    {
        public InventorySlotGUI slotPrefab;
        public Dictionary<string, InventorySlotGUI> slots = new Dictionary<string, InventorySlotGUI>();
        public Button thingToConnectNavigationTo;

        public static InventoryGUI instance;

        public InventorySlotGUI lastSelectedSlot;

        private void Awake()
        {
            instance = this;
        }

        public void OnEnable()
        {
            if (!Player.instance || !Player.instance.identity) return;
            UpdateSlots();
            if (slots.Count > 0)
            {
                slots.First().Value.GetComponent<Button>().Select();
                lastSelectedSlot = slots.First().Value;
            }
        }

        void UpdateSlots()
        {
            DungeonObject playerOb = Player.instance.identity;
            foreach (var item in playerOb.inventory.items)
            {
                InventorySlotGUI slot;
                bool hasItem = slots.TryGetValue(item.Key, out slot);
                if (hasItem)
                {
                    // Update quantity
                }
                else
                {
                    AddSlot(item.Value);
                }
            }

            var slotsToRemove = new List<KeyValuePair<string, InventorySlotGUI>>();
            foreach (var slot in slots)
            {
                if (!playerOb.inventory.items.ContainsKey(slot.Key))
                {
                    slotsToRemove.Add(slot);
                }
            }

            foreach (var kv in slotsToRemove)
            {
                RemoveSlot(kv.Key);
            }
        }

        public void Update()
        {
            if (!Player.instance.identity) return;

            UpdateSlots();
        }

        void RemoveSlot(string key)
        {
            Destroy(slots[key].gameObject);
            slots.Remove(key);
            UpdateIndexes();
        }

        void UpdateIndexes()
        {
            var slotsValues = slots.Values.AsReadOnlyList();
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotGUI slot = slotsValues[i];
                InventorySlotGUI nextSlot = null;
                if (i + 1 < slots.Count)
                {
                    nextSlot = slotsValues[i + 1];
                }
                var thingNav = thingToConnectNavigationTo.navigation;
                var slotButton = slot.GetComponent<Button>();
                var slotNav = slotButton.navigation;
                if (i == 0)
                {
                    thingNav.selectOnRight = slotButton;
                    thingToConnectNavigationTo.navigation = thingNav;
                }

                slotNav.selectOnLeft = thingToConnectNavigationTo;

                if (nextSlot)
                {
                    var nextSlotButton = nextSlot.GetComponent<Button>();
                    var nextSlotNav = nextSlotButton.navigation;
                    slotNav.selectOnDown = nextSlotButton;
                    nextSlotNav.selectOnUp = slotButton;
                    nextSlotButton.navigation = nextSlotNav;
                }

                slotButton.navigation = slotNav;

                slot.index = i;
            }
        }

        void AddSlot(DungeonObject item)
        {
            var scrollView = GetComponent<ScrollRect>();
            var contentView = scrollView.content;
            var transform = contentView.transform;
            var slot = Instantiate(slotPrefab.gameObject, transform);
            slot.SetActive(true);
            var slotScript = slot.GetComponent<InventorySlotGUI>();
            slotScript.Init(item);

            slots.Add(item.objectName, slotScript);

            UpdateIndexes();
        }
    }
}