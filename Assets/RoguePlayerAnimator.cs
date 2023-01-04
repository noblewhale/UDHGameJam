using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        int health = healthProperty.GetValue();
        playerAnimator.SetFloat("Health", (float)health / creature.maxHealth);
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
