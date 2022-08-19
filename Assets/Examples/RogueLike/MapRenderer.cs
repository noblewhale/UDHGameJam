namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class MapRenderer : MonoBehaviour
    {
        public float vertical;
        public float horizontal;

        public static MapRenderer instance;
        public Material material;

        void Awake()
        {
            instance = this;
            material = GetComponent<MeshRenderer>().sharedMaterial;

        }

        private void Start()
        { 
        }
    }
}