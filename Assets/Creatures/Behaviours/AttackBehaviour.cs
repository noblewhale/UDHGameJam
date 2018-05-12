using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackBehaviour : MonoBehaviour
{
    protected Creature owner;

    virtual public void Awake()
    {
        owner = GetComponent<Creature>();
    }

    abstract public float ShouldAttack();
    abstract public void Attack();
}
