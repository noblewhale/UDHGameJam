namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class MapRenderer : MonoBehaviour
    {
        public float vertical;
        public float horizontal;

        public static MapRenderer instance;
        public Material warpMaterial;

        void Awake()
        {
            instance = this;
            warpMaterial = GetComponent<MeshRenderer>().material;
        }

        private void Start()
        { 
        }
    }
}