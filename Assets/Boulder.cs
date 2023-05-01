using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    
    void Start()
    {
        GetComponent<DungeonObject>().OnPreSteppedOn.AddListener(PushBoulder);
        Debug.Log("starting");
    }

    void PushBoulder(DungeonObject ob)
    {
        Debug.Log("pushing");
    }
}
