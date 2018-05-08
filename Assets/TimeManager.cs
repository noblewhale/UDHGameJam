using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static ulong time = 0;

    public static TimeManager instance;

    public void Awake()
    {
        instance = this;
    }

    public static void Tick(int numTicks)
    {
        for (int i = 0; i < numTicks; i++)
        {
            time++;

            foreach (var creature in CreatureSpawner.instance.allCreatures)
            {
                if (time >= creature.nextActionTime)
                {
                    creature.StartNewAction();
                }
                else
                {
                    creature.ContinueAction();
                }
            }
        }
    }
}
