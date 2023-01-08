namespace Noble.TileEngine
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(EventSystem))]
    public class EventSystemExtensions : MonoBehaviour
    {
        static EventSystem system;
        public static GameObject currentSelectedGameObject_Recent;
        public static GameObject LastSelectedGameObject;

        void Start()
        {
            system = EventSystem.current;
        }

        public void Update()
        {
            if (system.currentSelectedGameObject != currentSelectedGameObject_Recent)
            {
                LastSelectedGameObject = currentSelectedGameObject_Recent;
                currentSelectedGameObject_Recent = system.currentSelectedGameObject;
            }
        }

        public static Selectable GetNextSelectable(Selectable selectable, Vector2 axisMoveVector)
        {
            if (axisMoveVector.y < 0)
            {
                return selectable.navigation.selectOnDown;
            }
            else if (axisMoveVector.y > 0)
            {
                return selectable.navigation.selectOnUp;
            }
            else if (axisMoveVector.x < 0)
            {
                return selectable.navigation.selectOnLeft;
            }
            else // if (axisMoveVector.x > 0)
            {
                return selectable.navigation.selectOnRight;
            }
        }

        public static IEnumerator DelaySelect(Selectable nextSelectable)
        {
            yield return new WaitForEndOfFrame();
            nextSelectable.Select();
        }
    }
}
