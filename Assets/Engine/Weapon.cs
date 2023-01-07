namespace Noble.TileEngine
{
    using UnityEngine;

    [RequireComponent(typeof(Equipable))]
    public class Weapon : TickableBehaviour
    {
        public int minBaseDamage = 1;
        public int maxBaseDamage = 2;

        public int enchantmentLevel = 0;

        public Creature targetCreature;

        public Equipable Weildable { get; private set; }

        override public void Awake()
        {
            base.Awake();
            Weildable = GetComponent<Equipable>();
        }

        public override void FinishSubAction(ulong time)
        {
            if (!targetCreature) return;

            float roll = Random.Range(0, 20);
            int attackerDex = GetComponent<Equipable>().EquippedBy.baseObject.GetProperty<int>("Dexterity").GetValue();
            int defenderDex = targetCreature.baseObject.GetProperty<int>("Dexterity").GetValue();
            int defenderArmor = targetCreature.baseObject.GetProperty<int>("Armor").GetValue();
            roll += attackerDex;
            if (roll > defenderDex)
            {
                // Hit, but do we do damange?
                if (roll > defenderDex + defenderArmor)
                {
                    // Got past armor / defense
                    targetCreature.baseObject.DamageFlash(1);

                    int damage = Random.Range(minBaseDamage, maxBaseDamage + 1);
                    targetCreature.baseObject.TakeDamage(damage);
                }
            }
        }

    }
}