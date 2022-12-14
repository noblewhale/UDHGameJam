using Noble.DungeonCrawler;
using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapCamera : MonoBehaviour
{
    void Update()
    {
        if (PlayerCamera.instance.transform.position.x > Map.instance.transform.position.x + Map.instance.TotalWidth / 2)
        {
            transform.localPosition = Vector3.left * Map.instance.TotalWidth;
        }
        else
        {
            transform.localPosition = Vector3.right * Map.instance.TotalWidth;
        }
    }
}
