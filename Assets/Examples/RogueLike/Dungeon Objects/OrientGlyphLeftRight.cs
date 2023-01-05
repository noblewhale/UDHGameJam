namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using UnityEngine;

    public class OrientGlyphLeftRight : MonoBehaviour
    {
        Creature owner;
        Vector3 originalScale;
        Vector3 originalPos;

        void Awake()
        {
            owner = GetComponentInParent<Creature>();
            originalScale = transform.localScale;
            originalPos = transform.localPosition;
        }

        void Update()
        {
            switch (owner.lastDirectionAttackedOrMoved)
            {
                case Direction.RIGHT:
                case Direction.UP_RIGHT:
                case Direction.DOWN_RIGHT:
                    transform.localScale = originalScale;
                    //transform.localPosition = new Vector3(-originalPos.x, originalPos.y, originalPos.z);
                    break;
                case Direction.LEFT:
                case Direction.UP_LEFT:
                case Direction.DOWN_LEFT:
                    transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z); 
                    //transform.localPosition = originalPos;
                    break;
            }

        }
    }
}