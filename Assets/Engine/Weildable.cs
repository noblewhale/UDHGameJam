namespace Noble.TileEngine
{
    using UnityEngine;

    public class Weildable : MonoBehaviour
    {
        public bool IsWeilded { get; private set; }
        public Creature WeildedBy { get; private set; }

        public Transform handle;

        public bool autoWeildRightHand = false;
        public bool autoWeildLeftHand = false;

        public void Weild(Creature weilder, Transform hand)
        {
            IsWeilded = true;
            WeildedBy = weilder;

            transform.parent = hand;
            Vector3 handlePos = transform.position;
            if (handle)
            {
                handlePos = handle.position;
            }
            transform.position = hand.transform.position - (handlePos - transform.position);
        }

        public void UnWeild()
        {
            IsWeilded = false;
            WeildedBy = null;
            transform.parent = null;
            transform.position = new Vector3(-666, -666, -666);
        }
    }
}