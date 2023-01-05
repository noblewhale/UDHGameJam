using Noble.TileEngine;
using UnityEngine;

public class PropertyIntValueGUI : MonoBehaviour
{
    public string propertyName;
    public TMPro.TMP_Text labelTextComponent;
    public TMPro.TMP_Text valueTextComponent;

    Property<int> property;

    void OnEnable()
    {
        if (!Player.instance) return;

        var dungeonObject = Player.instance.identity;

        if (!dungeonObject) return;

        property = dungeonObject.GetProperty<int>(propertyName);
        property.onValueChanged += UpdateDisplay;

        UpdateDisplay(null, 0, 0);
    }

    private void OnDisable()
    {
        if (!property) return;

        property.onValueChanged -= UpdateDisplay;
    }

    void UpdateDisplay(Property<int> changedProperty, int oldValue, int newValue)
    {
        if (!property) return;

        labelTextComponent.text = property.propertyName;
        valueTextComponent.text = property.GetValue().ToString();
    }
}
