using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonObject : MonoBehaviour
{
    public string objectName;

    public SpriteRenderer[] glyphs;
    protected Color[] originalGlyphColors;
    protected Vector3[] originalGlyphPositions;
    public Map map;
    public int x;
    public int y;
    public int quantity;
    public bool canBePickedUp;

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
    Coroutine damageFlashProcess;

    virtual protected void Awake()
    {
        map = FindObjectOfType<Map>();

        glyphs = GetComponentsInChildren<SpriteRenderer>();
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
            for (int i = 0; i < glyphs.Length; i++)
            {
                if (!map.tileObjects[y][x].isInView) glyphs[i].color = originalGlyphColors[i] / 2;
                else glyphs[i].color = originalGlyphColors[i];
            }
        }
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
            glyphs[i].color = Color.white;
        }

        yield return new WaitForSeconds(.15f);

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

    virtual public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        transform.localPosition = new Vector3(x * map.tileWidth, y * map.tileHeight, transform.localPosition.z);
    }
}
