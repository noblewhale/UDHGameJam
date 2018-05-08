using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientedGlyph : MonoBehaviour
{
    Creature owner;

	void Start ()
    {
        owner = GetComponentInParent<Creature>();
	}
	
	void Update ()
    {
        switch (owner.lastDirectionMoved)
        {
            case Direction.UP:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.DOWN:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction.RIGHT:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case Direction.LEFT:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
        
	}
}
