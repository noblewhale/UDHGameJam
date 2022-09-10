namespace Noble.TileEngine
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [ExecuteInEditMode]
    public class BiomeObject : MonoBehaviour
    {
        public Biome biome;

        [SerializeField]
        public RectIntExclusive area;
        public List<BiomeObject> subBiomes = new List<BiomeObject>();

        virtual public void DrawDebug()
        {
            if (biome == null) return;

            biome.DrawDebug(new RectIntExclusive(Mathf.FloorToInt(transform.localPosition.x + area.xMin), Mathf.FloorToInt(transform.localPosition.y + area.yMin), area.width, area.height));
            //EditorUtil.DrawRect(Map.instance, new RectIntExclusive(Mathf.FloorToInt(transform.localPosition.x + area.xMin), Mathf.FloorToInt(transform.localPosition.y + area.yMin), area.width, area.height), Color.green);
        }

        virtual public IEnumerator PreProcessMap(Map map)
        {
            Debug.Log("Preprocess biome object");
            if (biome == null) yield break;

            yield return biome.PreProcessMap(map, this);
            foreach (var subBiome in subBiomes)
            {
                yield return subBiome.PreProcessMap(map);
            }
        }

        virtual public bool Contains(Vector2 pos)
        {
            var relativePos = new Vector2Int(Mathf.FloorToInt(pos.x - transform.position.x), Mathf.FloorToInt(pos.y - transform.position.y));
            return area.Contains(relativePos);
        }
    }

}