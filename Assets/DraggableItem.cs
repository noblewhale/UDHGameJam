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
            if (selectOnDrag)
            {
                GetComponent<Selectable>().Select();
            }
            draggableCopy = Instantiate(itemToDrag);
            var glyphsComponent = draggableCopy.GetComponentInChildren<Glyphs>(true);
            glyphsComponent.GetComponentInChildren<Glyphs>(true).enabled = false;
            glyphsComponent.gameObject.SetActive(true);
            glyphsComponent.SetLit(true);
            foreach (var glyph in glyphsComponent.glyphs)
            {
                glyph.GetComponent<SpriteRenderer>().color = glyph.originalColor;
            }
            foreach (Transform trans in draggableCopy.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = this.gameObject.layer;
            }
            draggableCopy.transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + new Vector3(-.5f, -.5f, 1);
        }

        public void OnDrag(PointerEventData eventData)
        {
            draggableCopy.transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + new Vector3(-.5f, -.5f, 1);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Destroy(draggableCopy);
        }
    }

}