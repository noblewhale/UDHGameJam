namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class PropertyOxygen : PropertyInt
    {
        public bool isDrowning = false;
        public bool isHUDVisable = false;
        private bool hideHUDNextTick = false;

        public override void FinishAction()
        {
            base.FinishAction();

            if (hideHUDNextTick)
            {
                isHUDVisable = false;
                hideHUDNextTick = false;
            }

            // Create bool for drowning if player is in a water tile or "DrowningTrap"
            isDrowning = owner.tile.ContainsObjectWithComponent<DrowningTrap>();

            if (isDrowning)
            {
                // Show Oxygen Counter
                isHUDVisable = true;

                // If player is in water tile take away one from oxygen counter
                SetValue(GetValue() - 1);

                // If oxygen counter is less than -1 then start taking damage
                if (GetValue() <= -1)
                {
                    // Gets players current max health
                    var maxHealth = owner.GetPropertyValue<int>("Max Health");
                    // Sets damage to take at half of the players current health
                    var drowningDamage = Mathf.FloorToInt(maxHealth * .5f);
                    // Deals the drowning damage to the player
                    owner.TakeDamage(drowningDamage);
                }
            }
            else
            {
                hideHUDNextTick = true;

                // If player is not in "DrowningTrap" then reset Oxygen Counter
                SetValue(3); 
            }
        }


    }
}
