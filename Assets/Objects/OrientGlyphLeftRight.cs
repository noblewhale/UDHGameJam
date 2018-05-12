using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientGlyphLeftRight : MonoBehaviour
{
    Creature owner;
    Vector3 originalScale;

    void Start()
    {
        owner = GetComponentInParent<Creature>();
        originalScale = owner.transform.localScale;
    }

    void Update()
    {
        switch (owner.lastDirectionAttackedOrMoved)
        {
            case Direction.RIGHT:
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
                break;
            case Direction.LEFT:
                transform.localScale = originalScale;
                break;
        }

    }
}