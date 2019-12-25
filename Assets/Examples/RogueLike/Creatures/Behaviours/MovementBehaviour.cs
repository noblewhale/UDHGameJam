using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBehaviour : MonoBehaviour
{
    protected DungeonObject owner;

    virtual public void Awake()
    {
        owner = GetComponent<DungeonObject>();
    }

    abstract public float ShouldMove();
    abstract public void Move();
}
