namespace Noble.TileEngine
{
    using UnityEngine;

    public class PropertyCreatureArmor : PropertyInt
    {
        private void Awake()
        {
            UpdateValue();
            GetComponent<DungeonObject>().Equipment.onChange += UpdateValue;
        }

        void UpdateValue()
        {
            var equipment = GetComponent<DungeonObject>().Equipment.GetEquipment();
            int newValue = 0;
            foreach (var item in equipment)
            {
                if (item == null) continue;
                var armorProperty = item.DungeonObject.GetProperty<int>("Armor");
                if (!armorProperty) continue;
                newValue += armorProperty.GetValue();
            }

            SetValue(newValue);
        }
    }
}
