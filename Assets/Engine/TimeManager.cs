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

    public List<Tickable> tickableObjects = new List<Tickable>();

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

        for (int t = 0; t < nextTicks; t++)
        {
            time++;

            for (int oi = TimeManager.instance.tickableObjects.Count - 1; oi >= 0; oi--)
            {
                var ob = TimeManager.instance.tickableObjects[oi];
                if (time >= ob.nextActionTime)
                {
                    ob.StartNewAction();
                }
                else
                {
                    ob.ContinueAction();
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
