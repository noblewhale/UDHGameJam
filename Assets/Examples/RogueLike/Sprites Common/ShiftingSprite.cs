namespace Noble.DungeonCrawler
{
    using UnityEngine;

    public class ShiftingSprite : MonoBehaviour
    {

        public float speed = .5f;
        public float maxOffset = .5f;
        public float timeOffset = 0;

        void LateUpdate()
        {
            float offset = Mathf.Sin((Time.time + timeOffset) * speed) * maxOffset;
            GetComponent<SpriteRenderer>().material.SetVector("_Offset", new Vector4(offset, offset, 0, 0));
        }
    }
}
