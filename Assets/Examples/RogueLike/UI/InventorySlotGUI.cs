namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using TMPro;
    using UnityEngine;

    public class InventorySlotGUI : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Transform glyphParent;
        public int index;
        public DungeonObject item;

        RectTransform rect;

        public void Init(DungeonObject item)
        {
            this.item = item;
            rect = GetComponent<RectTransform>();
            transform.localRotation = Quaternion.identity;
            label.text = item.objectName;
            if (item.guiIcon)
            {
                foreach (var iconPart in item.guiIcon.GetComponentsInChildren<RectTransform>())
                {
                    var imageOb = Instantiate(iconPart.gameObject);
                    var imageRect = imageOb.GetComponentInParent<RectTransform>();
                    imageRect.SetParent(glyphParent, false);
                    //imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.height * iconPart.transform.localScale.y);
                    //imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.height);
                    imageOb.layer = gameObject.layer;
                }
            }
        }

        public void Update()
        {
            rect.anchoredPosition = Vector3.down * index * rect.rect.height;
            //transform.localPosition = Vector3.down * index * .2f;
            Weapon weapon = item.GetComponent<Weapon>();
            if (weapon != null && weapon.Weildable.IsWeilded)
            {
                label.text = "[" + item.objectName + "]";
            }
            else
            {
                label.text = item.objectName;
            }
        }
    }
}