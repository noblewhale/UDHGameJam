namespace Noble.TileEngine
{
    using UnityEngine;

    [RequireComponent(typeof(DungeonObject))]
    public class PropertyHealth : PropertyInt
    {
        public void Awake()
        {
            GetComponent<DungeonObject>().onTakeDamage += OnTakeDamage;
        }

        void OnTakeDamage(int damage)
        {
            int newHealth = Mathf.Max(0, _value - damage);
            SetValue(newHealth);

            if (newHealth == 0)
            {
                GetComponent<DungeonObject>().Die();
            }
        }
    }
}
