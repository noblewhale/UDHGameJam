namespace Noble.TileEngine
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public interface IProperty
    {
        public string propertyName { get; set; }
        public object GetValueUntyped();
        public void SetValueUntyped(object value);
    }

    [Serializable]
    public class PropertyModifier<T>
    {
        public string propertyName;
        public ulong duration = 0;
        [NonSerialized] public ulong timeAdded = 0;
        virtual public T Modify(T input) { return input; }
    }

    public class Property<T> : TickableBehaviour, IProperty
    {
        [SerializeField]
        protected T _value;

        [SerializeField]
        public List<PropertyModifier<T>> modifiers = new();

        [SerializeField]
        protected string _propertyName;
        public string propertyName { get => _propertyName; set => _propertyName = value; }

        public event Action<Property<T>, T, T> onValueChanged;


        public override void Awake()
        {
            base.Awake();
            executeEveryTick = true;
        }

        public override void StartSubAction(ulong time)
        {
            base.StartSubAction(time);
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (TimeManager.instance.Time - modifiers[i].timeAdded >= modifiers[i].duration)
                {
                    modifiers.RemoveAt(i);
                }
            }
        }

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
            T v = _value;
            foreach (var modifier in modifiers)
            {
                v = modifier.Modify(v);
            }
            return v;
        }

        public void SetValue(T value)
        {
            T oldValue = _value;
            _value = value;
            onValueChanged?.Invoke(this, oldValue, _value);
        }

        public void AddModifier(PropertyModifier<T> modifier)
        {
            modifier.timeAdded = TimeManager.instance.Time;
            modifiers.Add(modifier);
        }

        public void RemoveModifier(PropertyModifier<T> modifier)
        {
            modifiers.Remove(modifier);
        }
    }
}