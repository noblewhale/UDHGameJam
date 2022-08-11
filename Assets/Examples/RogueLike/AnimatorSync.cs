namespace Noble.DungeonCrawler
{
    using UnityEngine;


    public class AnimatorSync : MonoBehaviour
    {
        public float speedMultiplier = 1;
        public float offsetBasedOnYPos = 0;
        Animator globalAnimator;
        Animator animator;

        private void Awake()
        {
            globalAnimator = GameObject.Find("GlobalAnimator").GetComponent<Animator>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (globalAnimator && animator)
            {
                animator.SetFloat("Time", globalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * speedMultiplier + transform.position.y * offsetBasedOnYPos);
            }
        }
    }
}
