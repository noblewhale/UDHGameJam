namespace Noble.DungeonCrawler
{
    using UnityEngine;
    using Noble.TileEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public GameObject itemToDrag;
        public GameObject draggableCopy;
        public bool selectOnDrag = false;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (selectOnDrag)
            {
                GetComponent<Selectable>().Select();
            }
            draggableCopy = Instantiate(itemToDrag, null);
            draggableCopy.transform.localScale = Vector3.one;
            var glyphsComponent = draggableCopy.GetComponentInChildren<Glyphs>(true);
            glyphsComponent.GetComponentInChildren<Glyphs>(true).enabled = false;
            glyphsComponent.gameObject.SetActive(true);
            glyphsComponent.SetLit(true);
            foreach (var glyph in glyphsComponent.glyphs)
            {
                glyph.GetComponent<SpriteRenderer>().color = glyph.originalColor;
                glyph.GetComponent<SpriteRenderer>().sortingOrder = 999;
            }
            foreach (Transform trans in draggableCopy.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = this.gameObject.layer;
            }
            var combinedBounds = draggableCopy.GetCombinedBounds();
            draggableCopy.transform.localScale = Vector3.one / Mathf.Max(combinedBounds.size.x, combinedBounds.size.y);
            draggableCopy.transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + new Vector3(-draggableCopy.transform.localScale.x/2, -draggableCopy.transform.localScale.y/2, 1);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggableCopy)
            {
                draggableCopy.transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + new Vector3(-draggableCopy.transform.localScale.x / 2, -draggableCopy.transform.localScale.y / 2, 1);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggableCopy)
            {
                Destroy(draggableCopy);
            }
        }
    }

}