namespace Noble.DungeonCrawler
{
    using UnityEngine;
    using Noble.TileEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public GameObject itemToDrag;
        public GameObject draggableCopy;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            draggableCopy = Instantiate(itemToDrag, null);
            draggableCopy.transform.localScale = Vector3.one;
            var glyphsComponent = draggableCopy.GetComponentInChildren<Glyphs>(true);
            glyphsComponent.enabled = false;
            glyphsComponent.gameObject.SetActive(true);
            glyphsComponent.SetLit(true);
            foreach (var glyph in glyphsComponent.glyphs)
            {
                glyph.sprite.color = glyph.originalColor;
                glyph.sprite.sortingOrder = 999;
            }
            foreach (Transform trans in draggableCopy.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = gameObject.layer;
            }
            var combinedBounds = draggableCopy.GetCombinedBounds();
            draggableCopy.transform.localScale = Vector3.one / Mathf.Max(combinedBounds.size.x, combinedBounds.size.y);

            SetCopyPosition();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!draggableCopy) return;

            SetCopyPosition();
        }

        void SetCopyPosition()
        {
            var mousePos = Mouse.current.position.ReadValue();
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            var center = (Vector2)draggableCopy.transform.localScale / 2;
            draggableCopy.transform.position = mouseWorldPos - (Vector3)center + Vector3.forward;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggableCopy) return;

            Destroy(draggableCopy);
        }
    }

}