using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBehaviour : MonoBehaviour
{
    public Creature owner;

    public void Awake()
    {
        owner = GetComponent<Creature>();
    }

    abstract public float ShouldMove();
    abstract public void Move();
}
