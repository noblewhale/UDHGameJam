﻿namespace Noble.TileEngine
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerInput : MonoBehaviour
    {
        public Queue<Command> commandQueue = new Queue<Command>();

        public bool HasInput
        {
            get => commandQueue.Count != 0;
        }

        KeyCode[] allKeys;

        public static PlayerInput instance;

        public virtual void Awake()
        {
            instance = this;
            allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
        }

        public virtual void Update()
        {
            if (!Player.instance.identity) return;
            CollectInputCommands();
            if (HasInput)
            {
                TimeManager.instance.Interrupt(Player.instance.identity.GetComponent<Tickable>());
            }
        }

        protected virtual void CollectInputCommands()
        {
            foreach (KeyCode key in allKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    Command command = new Command { key = key };
                    if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1)
                    {
                        command.target = Input.mousePosition;
                    }
                    commandQueue.Enqueue(command);
                }
            }
        }
    }
}