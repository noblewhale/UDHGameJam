using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonObject : MonoBehaviour
{
    public Map map;
    public int tileX;
    public int tileY;
    
    public int defense = 1;
    public int health = 1;
    public bool isCollidable = true;
    public bool blocksLineOfSight = false;

    virtual protected void Awake()
    {
        map = FindObjectOfType<Map>();
    }

    // Update is called once per frame
    virtual public void Update ()
    {
        transform.localPosition = new Vector3(tileX * map.tileWidth, tileY * map.tileHeight, transform.localPosition.z);
    }

    virtual public void Collide() { }
    
    public void TakeDamage(int v)
    {
        health -= v;
        if (health < 0) health = 0;
        if (health == 0)
        {
            Die();
        }
    }

    virtual public void Die()
    {
        map.tileObjects[tileY][tileX].objectStack.Remove(this);
        Destroy(gameObject);
    }
}
