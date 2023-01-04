using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoguePlayerAnimator : MonoBehaviour
{
    Animator playerAnimator;
    Creature creature;

    private void Awake()
    {
        creature = GetComponent<Creature>();
        playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        playerAnimator.SetFloat("Health", (float)creature.health / creature.maxHealth);
    }
}
