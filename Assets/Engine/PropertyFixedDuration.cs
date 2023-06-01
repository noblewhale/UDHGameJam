using UnityEngine;

namespace Noble.TileEngine
{
    public class PropertyFixedDuration : Property<int>
    {
        public override void FinishAction()
        {
            base.FinishAction();

            SetValue(GetValue() - 1);

            if (GetValue() <= 0)
            {
                owner.RemoveProperty(this);
            }
        }
    }
}
