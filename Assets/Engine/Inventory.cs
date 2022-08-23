namespace Noble.TileEngine
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Inventory
    {
        public Dictionary<string, DungeonObject> items = new Dictionary<string, DungeonObject>();

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

        public void DestroyAll()
        {
            Debug.LogWarning("Inventory destroy");
            foreach (var kv in items)
            {
                GameObject.Destroy(kv.Value.gameObject);
            }

            items.Clear();
        }
    }
}
