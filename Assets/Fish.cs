using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{

    public void FishCrush(DungeonObject stepper)
    {

        if (stepper.objectName == "Boulder")
        {


            var threatenedFish = GetComponent<DungeonObject>();
            var threatenedFishTile = threatenedFish.tile;

            threatenedFishTile.RemoveObject(threatenedFish, true);

        }


    }





}
