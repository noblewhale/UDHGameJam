namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class PropertyOxygen : PropertyInt
    {
        public override void StartAction()
        {
            base.StartAction();

            bool isDrowning = owner.tile.ContainsObjectWithComponent<DrowningTrap>();

            if (isDrowning)
            {
                SetValue(GetValue() - 1);

                if (GetValue() <= -1)
                {
                    owner.TakeDamage(5);
                }

                Debug.Log("Glub");
            }
            else
            {
                SetValue(3);

                Debug.Log("Inhale");
            }
        }


    }
}
