using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGUI : MonoBehaviour
{
    public InventorySlotGUI slotPrefab;
    public Dictionary<string, InventorySlotGUI> slots = new Dictionary<string, InventorySlotGUI>();

    public void Update()
    {
        Creature playerCreature = Player.instance.identity;
        foreach (var item in playerCreature.inventory.items)
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
            if (!playerCreature.inventory.items.ContainsKey(slot.Key))
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
        slot.transform.localRotation = Quaternion.identity;
        slot.label.text = item.objectName;
        DungeonObject glyph = Instantiate(item.gameObject, slot.glyphParent).GetComponent<DungeonObject>();
        glyph.isAlwaysLit = true;
        SetLayerRecursive(glyph.gameObject, gameObject.layer);

        glyph.transform.localPosition = Vector3.zero;
        slots.Add(item.objectName, slot);

        UpdateIndexes();
    }
    
    public void SetLayerRecursive(UnityEngine.GameObject go, int layer)
    {
        if (go == null) return;
        foreach (var trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layer;
        }
    }
}
