namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class ContinueButton : MonoBehaviour
    {
        public GameObject window;

        public void Start()
        {
            PlayerInputHandler.instance.enabled = false;
        }

        public void Continue()
        {
            window.SetActive(false);
            PlayerInputHandler.instance.enabled = true;
        }
    }
}