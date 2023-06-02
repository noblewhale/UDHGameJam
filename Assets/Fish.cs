using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    DungeonObject threatenedFish;
    Tile threatenedFishTile;
    List<Tile> adjacentWater = new();

    public void FishCrush(DungeonObject stepper)
    {
        threatenedFish = GetComponent<DungeonObject>();
        threatenedFishTile = threatenedFish.tile;
        adjacentWater.Clear();

        GetAdjacentWaterTile(-1,0);
        GetAdjacentWaterTile(1, 0);
        GetAdjacentWaterTile(0, -1);
        GetAdjacentWaterTile(0, 1);

        if (adjacentWater.Count == 0)
        {
            if (stepper.objectName == "Boulder")
            {
                threatenedFishTile.RemoveObject(threatenedFish, true);
            }
        }
        else
        {
            var possibleWaterDestination = UnityEngine.Random.Range(0,adjacentWater.Count);

            //Map.instance.MoveObject(threatenedFish, adjacentWater[possibleWaterDestination].tilePosition);
        }

        
    }

    void GetAdjacentWaterTile(int x, int y)
    {
        var adjacentTile = Map.instance.GetTile(threatenedFishTile.x + x, threatenedFishTile.y + y);

        if (adjacentTile.ContainsObjectOfType("Water"))
        {
            adjacentWater.Add(adjacentTile);
        }
    }





}
