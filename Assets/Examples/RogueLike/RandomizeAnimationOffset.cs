namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class RandomizeAnimationOffset : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<Animator>().SetFloat("CycleOffset", Random.value);
        }
    }
}