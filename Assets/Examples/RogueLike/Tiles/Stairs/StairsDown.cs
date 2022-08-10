using UnityEngine;

public class StairsDown : MonoBehaviour
{
    public void OnSteppedOn(Creature creature)
    {
        if (Player.instance.identity == creature)
        {
            Map.instance.StartCoroutine(Map.instance.RegenerateMap());
        }
    }
}
