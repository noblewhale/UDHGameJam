namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class MapRenderer : MonoBehaviour
    {
        public static MapRenderer instance;

        public Material warpMaterial;

        virtual public void Awake()
        {
            instance = this;
            warpMaterial = GetComponent<MeshRenderer>().material;
        }

        virtual public void PlaceQuad()
        {

        }
    }
}
