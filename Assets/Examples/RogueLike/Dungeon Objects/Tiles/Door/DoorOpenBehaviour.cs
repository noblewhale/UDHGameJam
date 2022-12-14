namespace Noble.DungeonCrawler
{
    using Noble.TileEngine;
    using System.Collections;
    using UnityEngine;

    public class DoorOpenBehaviour : TickableBehaviour
    {
        public Animator animator;

        public override void StartAction()
        {
            animator.speed = 1;
            animator.SetTrigger("Open");
            owner.tickable.nextActionTime = TimeManager.instance.Time + 1;
        }

        public override void StartSubAction(ulong time)
        {
            Debug.Log("Start please");
            animator.speed = 1;
            //if (time == 1)
            {
                GetComponent<Door>().isCollidable = false;
            }
        }

        public override bool ContinueSubAction(ulong time)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("DoorOpen")) return false;

            Debug.Log("continue please");
            float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            //if (time == 0)
            //{
            //    return animationTime >= .2f;
            //}
            //else 
            {
                return animationTime >= 1;
            }
        }

        public override void FinishSubAction(ulong time)
        {

            Debug.Log("Finished");
            //if (time == 0)
            //{
            //    animator.Play(0, 0, .2f);
            //}
            //else
            {
                animator.Play(0, 0, 1);
            }
            animator.speed = 0;
        }
    }
}
