namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class OxygenHUDCounter : MonoBehaviour
    {
        public GameObject OxygenCounter;
        public GameObject Bubble1;
        public GameObject Bubble2;
        public GameObject Bubble3;

        private void Update()
        {
            var drownProperty = ((PropertyOxygen)Player.Identity.GetProperty<int>("Oxygen"));
            var drowning = drownProperty.isHUDVisable;
            var drownValue = drownProperty.GetValue();

            if (drowning)
            {
                OxygenCounter.SetActive(true);
                switch (drownValue)
                {
                    case >= 2:
                        Bubble1.SetActive(true);
                        Bubble2.SetActive(true);
                        Bubble3.SetActive(true);
                        break;
                    case 1:
                        Bubble1.SetActive(true);
                        Bubble2.SetActive(true);
                        Bubble3.SetActive(false);
                        break;
                    case 0:
                        Bubble1.SetActive(true);
                        Bubble2.SetActive(false);
                        Bubble3.SetActive(false);
                        break;
                    case <= -1:
                        Bubble1.SetActive(false);
                        Bubble2.SetActive(false);
                        Bubble3.SetActive(false);
                        break;
                }
            }
            else
            {
                OxygenCounter.SetActive(false);
            }
        }
    }
}