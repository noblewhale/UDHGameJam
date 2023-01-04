namespace Noble.TileEngine
{
    using System;
    using UnityEngine;

    public interface IProperty
    {
        public string propertyName { get; set; }
        public object GetValueUntyped();
        public void SetValueUntyped(object value);
    }

    public class Property<T> : MonoBehaviour, IProperty
    {
        [SerializeField]
        protected T _value;

        [SerializeField]
        protected string _propertyName;
        public string propertyName { get => _propertyName; set => _propertyName = value; }

        public event Action<Property<T>, T, T> onValueChanged;

        public object GetValueUntyped()
        {
            return _value;
        }

        public void SetValueUntyped(object value)
        {
            T oldValue = _value;
            _value = (T)value;
            onValueChanged?.Invoke(this, oldValue, _value);
        }

        public T GetValue()
        {
            return _value;
        }

        public void SetValue(T value)
        {
            T oldValue = _value;
            _value = value;
            onValueChanged?.Invoke(this, oldValue, _value);
        }
    }
}