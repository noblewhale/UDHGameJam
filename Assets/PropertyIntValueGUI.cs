using Noble.TileEngine;
using UnityEngine;

public class PropertyIntValueGUI : MonoBehaviour
{
    public string propertyName;
    public TMPro.TMP_Text labelTextComponent;
    public TMPro.TMP_Text valueTextComponent;

    void OnEnable()
    {
        if (!Player.instance) return;

        var dungeonObject = Player.instance.identity;

        if (!dungeonObject) return;

        var property = dungeonObject.GetProperty<int>(propertyName);
        labelTextComponent.text = property.propertyName;
        valueTextComponent.text = property.GetValue().ToString();
    }
}
