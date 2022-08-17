namespace Noble.TileEngine
{
    using UnityEngine;

    public class Weildable : MonoBehaviour
    {
        public bool IsWeilded { get; private set; }
        public Creature WeildedBy { get; private set; }

        public Transform handle;

        public void Weild(Creature weilder, Transform hand)
        {
            IsWeilded = true;
            WeildedBy = weilder;

            transform.parent = hand;
            transform.localPosition = -(handle.position - transform.position);
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