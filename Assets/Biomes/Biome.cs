using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Biome
{

    public BiomeType biomeType;
    public RectInt area;

    public static DungeonObject SelectRandomObject(List<BiomeDropRate> rates)
    {
        // Get the total of all probabilities
        float totalProbability = rates.Sum(pob => pob.probability);
        // Generate a random number within the probability range
        float aRandomNumber = Random.Range(0, totalProbability);

        // Randomly select an object such that objects with a higher probability are more likely to be selected
        float currentProbability = 0;
        float previousProbability = 0;
        DungeonObject selectedObject = null;
        foreach (var probability in rates)
        {
            previousProbability = currentProbability;
            currentProbability += probability.probability;
            if (aRandomNumber >= previousProbability && aRandomNumber < currentProbability)
            {
                selectedObject = probability.item;
                break;
            }
        }

        // TODO: Respect min and max and shit
        return selectedObject;
    }
}
