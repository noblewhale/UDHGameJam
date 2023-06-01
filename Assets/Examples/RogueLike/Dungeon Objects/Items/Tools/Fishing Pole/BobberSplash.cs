namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class BobberSplash : MonoBehaviour
    {
        public FishingBehaviour fishingParameter;

        public void OnBob()
        {

            fishingParameter.bobCount++;
            fishingParameter.didBob = true;
        }

        public void OnFishCaught()
        {
            fishingParameter.FishCaught();
        }
    }
}
