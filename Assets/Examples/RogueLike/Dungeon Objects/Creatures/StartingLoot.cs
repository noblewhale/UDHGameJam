namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class StartingLoot : MonoBehaviour
    {
        Creature owner;

        public DropRate[] itemRates;

        virtual public void Start()
        {
            owner = GetComponent<Creature>();
            foreach (var dropRate in itemRates)
            {
                var r = Random.value;

                if (r <= dropRate.probability)
                {
                    int quantity = Random.Range(dropRate.minQuantity, dropRate.maxQuantity + 1);
                    var item = Instantiate(dropRate.item.gameObject).GetComponent<DungeonObject>();
                    item.transform.position = new Vector3(-666, -666, -666);
                    item.quantity = quantity;
                    //owner.inventory.items.Add(item.objectName, item);
                    owner.baseObject.AddToInventory(item);
                }
            }
        }
    }
}