using UnityEngine;

public class StairsDown : MonoBehaviour
{
    public void OnSteppedOn(Creature creature)
    {
        if (Player.instance.identity == creature)
        {
            Player.instance.map.StartCoroutine(Player.instance.map.RegenerateMap());
        }
    }
}
