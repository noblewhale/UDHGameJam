namespace Noble.DungeonCrawler {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TileEngine;


    public class HP : MonoBehaviour
    {
        float hpBarMaxWidth;


        // Start is called before the first frame update
        void Awake()
        {
            hpBarMaxWidth = GetComponent<RectTransform>().rect.width;
            Debug.Log(hpBarMaxWidth);
        }

        // Update is called once per frame
        void Update()
        {
            float hp = Player.Identity.GetPropertyValue<int>("Health");
            float maxHP = Player.Identity.GetPropertyValue<int>("Max Health");

            Debug.Log("HP " + hp);
            Debug.Log("Max HP " + maxHP);

            var newBarLength = (hp/maxHP) * hpBarMaxWidth;

            GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newBarLength);

            Debug.Log(newBarLength);
        }
    }
}
