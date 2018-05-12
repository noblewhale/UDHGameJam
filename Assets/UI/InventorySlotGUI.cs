using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlotGUI : MonoBehaviour
{
    public TextMesh label;
    public Transform glyphParent;
    public int index;

    public void Update()
    {
        transform.localPosition = Vector3.down * index * .2f;
    }
}
