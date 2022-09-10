using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorOnlySprite : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject);
    }
}
