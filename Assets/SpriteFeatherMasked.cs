using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteFeatherMasked : MonoBehaviour
{
    public Sprite mask;
    MaterialPropertyBlock properties;

    private void OnEnable()
    {
        properties = new MaterialPropertyBlock();
        GetComponent<SpriteRenderer>().GetPropertyBlock(properties);
    }

    // Update is called once per frame
    void Update()
    {
        if (properties != null && mask != null)
        {
            properties.SetTexture("_Mask", mask.texture);
            GetComponent<SpriteRenderer>().SetPropertyBlock(properties);
        }
    }
}
