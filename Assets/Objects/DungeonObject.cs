using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DungeonObject : MonoBehaviour
{
    public string objectName;

    [Serializable]
    public class CreatureEvent : UnityEvent<Creature> { }
    
    public CreatureEvent OnSteppedOn;

    public SpriteRenderer[] glyphs;
    protected Color[] originalGlyphColors;
    protected Vector3[] originalGlyphPositions;
    public Map map;
    public int x;
    public int y;
    public int quantity = 1;
    public bool canBePickedUp;
    public bool isAlwaysLit;
    
    public bool isWeilded = false;

    public Inventory inventory = new Inventory();

    public int Gold {
        get {
            return inventory.Gold;
        }
    }

    public int defense = 1;
    public int health = 1;
    public bool isCollidable = true;
    public bool blocksLineOfSight = false;
    public bool coversObjectsBeneath = false;
    public bool preventsObjectSpawning = false;
    Coroutine damageFlashProcess;
    public Color damageFlashColor = Color.red;

    virtual protected void Awake()
    {
        map = FindObjectOfType<Map>();

        glyphs = GetComponentsInChildren<SpriteRenderer>(true);
        originalGlyphColors = new Color[glyphs.Length];
        originalGlyphPositions = new Vector3[glyphs.Length];
        for (int i = 0; i < glyphs.Length; i++)
        { 
            originalGlyphColors[i] = glyphs[i].color;
            originalGlyphPositions[i] = glyphs[i].transform.localPosition;
        }
    }

    // Update is called once per frame
    virtual public void Update ()
    {
        if (damageFlashProcess == null)
        {
            if (isAlwaysLit)
            {
                for (int i = 0; i < glyphs.Length; i++)
                {
                    glyphs[i].color = originalGlyphColors[i];
                }
            }
            else
            {
                for (int i = 0; i < glyphs.Length; i++)
                {
                    if (!map.tileObjects[y][x].isInView) glyphs[i].color = originalGlyphColors[i] / 2;
                    else glyphs[i].color = originalGlyphColors[i];
                }
            }
        }
    }

    virtual public void Collide(DungeonObject ob) { }

    public void SteppedOn(Creature creature)
    {
        OnSteppedOn.Invoke(creature);
    }
    
    public void TakeDamage(int v)
    {
        health -= v;
        if (health < 0) health = 0;
        if (health == 0)
        {
            Die();
        }

        if (glyphs.Length > 0)
        {
            if (damageFlashProcess != null) StopCoroutine(damageFlashProcess);
            damageFlashProcess = StartCoroutine(DoDamageFlash());
        }
    }

    IEnumerator DoDamageFlash()
    {
        for (int i = 0; i < glyphs.Length; i++)
        {
            glyphs[i].color = damageFlashColor;
        }

        yield return new WaitForSeconds(.2f);

        for (int i = 0; i < glyphs.Length; i++)
        {
            glyphs[i].color = originalGlyphColors[i];
        }
        damageFlashProcess = null;
    }

    virtual public void Die()
    {
        map.tileObjects[y][x].objectList.Remove(this);
        DropItems();
        Destroy(gameObject);
    }

    virtual public void DropItems()
    {
        foreach (var kv in inventory.items)
        {
            map.tileObjects[y][x].AddObject(kv.Value);
        }
    }

    virtual public void SetPosition(int x, int y, bool isAction = true)
    {
        this.x = x;
        this.y = y;
        transform.localPosition = new Vector3(x * map.tileWidth, y * map.tileHeight, transform.localPosition.z);
    }
}
