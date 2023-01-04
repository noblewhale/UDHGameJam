namespace Noble.TileEngine
{
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
                newValue += item.DungeonObject.GetProperty<int>("Armor").GetValue();
            }

            SetValue(newValue);
        }
    }
}
