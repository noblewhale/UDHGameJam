using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventorySlotGUI : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Transform glyphParent;
    public int index;
    public DungeonObject item;

    RectTransform rect;

    public void Init(DungeonObject item)
    {
        this.item = item;
        rect = GetComponent<RectTransform>();
        transform.localRotation = Quaternion.identity;
        label.text = item.objectName;
        foreach (var sprite in item.glyphs.glyphs)
        {
            var imageOb = new GameObject();
            var imageComp = imageOb.AddComponent<UnityEngine.UI.Image>();
            imageComp.color = sprite.color;
            imageComp.sprite = sprite.sprite;
            var imageRect = imageOb.GetComponentInParent<RectTransform>();
            imageRect.SetParent(glyphParent, false);
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.height);
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.height);
            imageOb.layer = gameObject.layer;
        }
    }

    public void Update()
    {
        rect.anchoredPosition = Vector3.down * index * rect.rect.height;
        //transform.localPosition = Vector3.down * index * .2f;
        if (item.isWeilded)
        {
            label.text = "["+item.objectName+"]";
        }
        else
        {
            label.text = item.objectName;
        }
    }
}
