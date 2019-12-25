using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackBehaviour : MonoBehaviour
{
    protected DungeonObject owner;

    virtual public void Awake()
    {
        owner = GetComponent<DungeonObject>();
    }

    abstract public float ShouldAttack();
    abstract public void Attack();
}
