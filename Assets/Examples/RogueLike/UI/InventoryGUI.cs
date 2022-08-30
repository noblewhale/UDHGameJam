namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections.Generic;
    using UnityEngine;

    public class InventoryGUI : MonoBehaviour
    {
        public InventorySlotGUI slotPrefab;
        public Dictionary<string, InventorySlotGUI> slots = new Dictionary<string, InventorySlotGUI>();

        public void Update()
        {
            if (!Player.instance.identity) return;

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

        void RemoveSlot(string key)
        {
            Destroy(slots[key].gameObject);
            slots.Remove(key);
            UpdateIndexes();
        }

        void UpdateIndexes()
        {
            int i = 0;
            foreach (var slot in slots)
            {
                slot.Value.index = i;
                i++;
            }
        }

        void AddSlot(DungeonObject item)
        {
            var slot = Instantiate(slotPrefab.gameObject, transform).GetComponent<InventorySlotGUI>();
            slot.Init(item);

            slots.Add(item.objectName, slot);

            UpdateIndexes();
        }
    }
}