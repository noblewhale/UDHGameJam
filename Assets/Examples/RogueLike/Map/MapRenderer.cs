namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System;
    using UnityEngine;

    public class MapRenderer : MonoBehaviour
    {
        public static MapRenderer instance;

        public Material warpMaterial;

        public float renderedWidth;
        public float renderedHeight;

        virtual public void Awake()
        {
            instance = this;
            warpMaterial = GetComponent<MeshRenderer>().material;
        }

        virtual public void Update()
        { 
            CreateRenderTexture();
            PlaceQuad();
        }

        virtual public void PlaceQuad()
        {

        }

        virtual public void CreateRenderTexture()
        {
        }
    }
}
