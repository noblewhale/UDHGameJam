namespace Noble.TileEngine
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Inventory : IReadOnlyDictionary<string, DungeonObject>
    {
        private DungeonObject owner;

        private Dictionary<string, DungeonObject> items = new Dictionary<string, DungeonObject>();
        public IEnumerable<string> Keys => items.Keys;
        public IEnumerable<DungeonObject> Values => items.Values;
        public int Count => items.Count;
        public DungeonObject this[string key] => items[key];

        public int Gold
        {
            get
            {
                DungeonObject ob;
                bool success = items.TryGetValue("Gold", out ob);
                if (success) return ob.quantity;
                else return 0;
            }
        }

        public Inventory(DungeonObject owner)
        {
            this.owner = owner;
        }

        public void DestroyAll()
        {
            foreach (var kv in items)
            {
                GameObject.Destroy(kv.Value.gameObject);
            }

            items.Clear();
        }

        public void AddItem(DungeonObject item)
        {
            DungeonObject existingOb;
            bool success = items.TryGetValue(item.objectName, out existingOb);
            if (success)
            {
                existingOb.quantity += item.quantity;
            }
            else
            {
                items.Add(item.objectName, item);
            }
            item.container = owner;
            item.transform.position = new Vector3(-666, -666, -666);
        }

        public DungeonObject RemoveItem(string nameOfItemToRemove, int quantityToRemove = 1)
        {
            bool hasitem = items.TryGetValue(nameOfItemToRemove, out DungeonObject item);
            if (hasitem)
            {
                item.quantity -= quantityToRemove;
                if (item.quantity <= 0)
                {
                    items.Remove(nameOfItemToRemove);
                    item.container = null;
                }
            }

            return item;
        }

        public bool ContainsKey(string key) => items.ContainsKey(key);

        public bool TryGetValue(string key, out DungeonObject value) => items.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<string, DungeonObject>> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

    }
}
