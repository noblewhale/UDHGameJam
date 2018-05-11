using UnityEngine;

public class StairsDown : MonoBehaviour
{
    public void OnSteppedOn(Creature creature)
    {
        Debug.Log("Stepped on stairs");
        if (Player.instance.identity == creature)
        {
            Debug.Log("do it");
            StartCoroutine(Player.instance.map.RegenerateMap());
        }
    }
}
