using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBehaviour : MonoBehaviour
{
    protected Creature owner;

    virtual public void Awake()
    {
        owner = GetComponent<Creature>();
    }

    abstract public float ShouldMove();
    abstract public void Move();
}
