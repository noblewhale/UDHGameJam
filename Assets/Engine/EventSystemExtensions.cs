namespace Noble.TileEngine
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    [RequireComponent(typeof(EventSystem))]
    public class EventSystemExtensions : MonoBehaviour
    {
        static EventSystem system;
        static GameObject currentSelectedGameObject_Recent;
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
    }
}
