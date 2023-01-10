namespace Noble.DungeonCrawler
{
    using UnityEngine;
    using Noble.TileEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // The item to make a copy of to show while dragging
        // The actual item itself is not used
        GameObject itemToDrag;

        // The actual object that gets dragged around, a copy of itemToDrag
        GameObject draggableCopy;

        public void Init(GameObject itemToDrag)
        {
            this.itemToDrag = itemToDrag;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Only left click can start a drag
            if (eventData.button != PointerEventData.InputButton.Left) return;

            // Copy the itemToDrag
            draggableCopy = Instantiate(itemToDrag, null);

            // Make sure no weird lighting stuff is happening
            var glyphsComponent = draggableCopy.GetComponentInChildren<Glyphs>(true);
            glyphsComponent.enabled = false;
            glyphsComponent.gameObject.SetActive(true);
            glyphsComponent.SetLit(true);
            foreach (var glyph in glyphsComponent.glyphs)
            {
                glyph.gameObject.SetActive(true);
                glyph.sprite.color = glyph.originalColor;
                // Dragged item appears in front of everything
                glyph.sprite.sortingOrder = 999;
            }

            // Move the copied item to the UI layer so it can be seen
            foreach (Transform trans in draggableCopy.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = gameObject.layer;
            }

            // Normalize the object size based on sprite bounds
            draggableCopy.transform.localScale = Vector3.one;
            var combinedBounds = draggableCopy.GetCombinedBounds();
            draggableCopy.transform.localScale = Vector3.one / Mathf.Max(combinedBounds.size.x, combinedBounds.size.y);

            // Set position based on mouse pos
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
            // Center the object on the mouse and move it in front of the camera's near plane.
            draggableCopy.transform.position = mouseWorldPos - (Vector3)center + Vector3.forward;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggableCopy) return;

            Destroy(draggableCopy);
        }
    }

}