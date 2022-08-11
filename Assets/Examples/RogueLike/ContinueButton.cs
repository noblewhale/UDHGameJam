namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class ContinueButton : MonoBehaviour
    {
        public GameObject window;

        public void Continue()
        {
            window.SetActive(false);
            PlayerCamera.instance.owner = Player.instance.identity;
            Player.instance.playerInput.enabled = true;
        }
    }
}