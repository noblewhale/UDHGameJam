using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acid : MonoBehaviour
{
    public void AcidDamage(DungeonObject stepper)
    {
        var stepperMaxHealth = stepper.GetPropertyValue<int>("Max Health");

        stepper.TakeDamage(stepperMaxHealth/2);
    }
}
