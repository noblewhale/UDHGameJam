using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static ulong time = 0;

    public static TimeManager instance;
    public static bool isBetweenTicks;
    static Coroutine tickProcess;
    static int nextTicks;
    public static event Action OnTick;

    public void Awake()
    {
        instance = this;
    }

    public static void Tick(int numTicks)
    {
        nextTicks = numTicks;
        if (tickProcess != null) instance.StopCoroutine(tickProcess);
        tickProcess = instance.StartCoroutine(WaitThenTick());
    }

    static IEnumerator WaitThenTick()
    {
        isBetweenTicks = true;
        float startTime = Time.time;
        while (Time.time - startTime < .15f)
            yield return new WaitForEndOfFrame();
        isBetweenTicks = false;

        _Tick();
    }

    private static void _Tick()
    {
        if (OnTick != null) OnTick();

        for (int i = 0; i < nextTicks; i++)
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

        tickProcess = null;
    }

    public static void Interrupt()
    {
        if (tickProcess != null)
        {
            instance.StopCoroutine(tickProcess);
            tickProcess = null;

            _Tick();
        }
    }
}
