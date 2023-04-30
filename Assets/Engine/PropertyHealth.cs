namespace Noble.TileEngine
{
    using UnityEngine;

    [RequireComponent(typeof(DungeonObject))]
    public class PropertyHealth : PropertyInt
    {
        override public void Awake()
        {
            base.Awake();
            GetComponent<DungeonObject>().onTakeDamage.AddListener(OnTakeDamage);
        }

        void OnTakeDamage(DungeonObject ob, int damage)
        {
            int newHealth = Mathf.Max(0, _value - damage);
            SetValue(newHealth);

            if (newHealth == 0)
            {
                ob.Die();
            }
        }
    }
}
