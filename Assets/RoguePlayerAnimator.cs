using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoguePlayerAnimator : MonoBehaviour
{
    Animator playerAnimator;
    Creature creature;
    float nextBlinkTime;

    private void Awake()
    {
        creature = GetComponent<Creature>();
        playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // TODO: These should really only be set when they change
        var healthProperty = creature.baseObject.GetProperty<int>("Health");
        var maxHealthProperty = creature.baseObject.GetProperty<int>("Max Health");
        int health = healthProperty.GetValue();
        int maxHealth = maxHealthProperty.GetValue(); ;
        playerAnimator.SetFloat("Health", (float)health / maxHealth);
        Equipable twoHandWeapon = creature.GetEquipment(Equipment.Slot.TWO_HANDED);
        if (twoHandWeapon)
        {
            playerAnimator.SetBool("IsTwoHanded", true);
        }
        else
        {
            playerAnimator.SetBool("IsTwoHanded", false);
        }

        if (Time.time > nextBlinkTime)
        {
            playerAnimator.SetTrigger("Blink");
            nextBlinkTime = Time.time + Random.Range(4f, 5f);
            if (Random.value < .1f) nextBlinkTime = Time.time + .33f;
        }
    }
}
