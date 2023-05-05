using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    
    void Start()
    {
        
    }

    public void PushBoulder(DungeonObject ob, bool isInstigator)
    {
        //reference to the boulder dungeon object
        DungeonObject baseObject = GetComponent<DungeonObject>();

        //Reference to boulder position 
        var boulderPosition = baseObject.tilePosition;

        //Reference to position of the thing pushing the boulder
        var pusherPosition = ob.tilePosition;

       
        var pushDirection = boulderPosition - pusherPosition;

        var updatedBoulderPosition = boulderPosition + pushDirection;

        Map.instance.TryMoveObject(baseObject, updatedBoulderPosition);

        var potentialNewTile = Map.instance.GetTile(updatedBoulderPosition);

        if (potentialNewTile.ContainsObjectOfType("Water") || potentialNewTile.ContainsObjectOfType("Acid"))
        {
            var waterToDestroy = potentialNewTile.GetObjectOfType("Water");
            var acidToDestroy = potentialNewTile.GetObjectOfType("Acid");
            
            
            potentialNewTile.RemoveObject(waterToDestroy, true);
           
            potentialNewTile.RemoveObject(acidToDestroy, true);
            
            potentialNewTile.RemoveObject(baseObject, true);
        }

        Debug.Log("pushing");
    }
}
