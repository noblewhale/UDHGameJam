using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlotGUI : MonoBehaviour
{
    public TextMesh label;
    public Transform glyphParent;
    public int index;
    public DungeonObject glyph;
    public DungeonObject item;

    public void Init(DungeonObject item)
    {
        this.item = item;
        transform.localRotation = Quaternion.identity;
        label.text = item.objectName;
        glyph = Instantiate(item.gameObject, glyphParent).GetComponent<DungeonObject>();
        glyph.isAlwaysLit = true;
        SetLayerRecursive(glyph.gameObject, gameObject.layer);

        glyph.transform.localPosition = Vector3.zero;
    }

    public void Update()
    {
        transform.localPosition = Vector3.down * index * .2f;
        if (item.isWeilded)
        {
            label.text = "["+item.objectName+"]";
        }
        else
        {
            label.text = item.objectName;
        }
    }
    
    public void SetLayerRecursive(UnityEngine.GameObject go, int layer)
    {
        if (go == null) return;
        foreach (var trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layer;
        }
    }
}
