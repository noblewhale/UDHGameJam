namespace Noble.DungeonCrawler
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class IconScaler : MonoBehaviour
    {
        void Update()
        {
            var parentRect = transform.parent.GetComponent<RectTransform>();
            if (parentRect)
            {
                var rect = GetComponent<RectTransform>();
                if (parentRect.rect.height > rect.rect.height && parentRect.rect.width > rect.rect.width)
                {
                    // If icon is smaller than parent area, scale it up
                    float aspect = rect.rect.width / rect.rect.height;
                    if (rect.rect.width >= rect.rect.height)
                    {
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentRect.rect.width / aspect);
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentRect.rect.width);
                    }
                    else
                    {
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentRect.rect.height);
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentRect.rect.height * aspect);
                    }
                }
            }
        }
    }
}