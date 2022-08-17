namespace Noble.TileEngine
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;

    public class PlayerInputHandler : MonoBehaviour
    {
        public Queue<Command> commandQueue = new Queue<Command>();

        public bool HasInput
        {
            get => commandQueue.Count != 0;
        }

        public static PlayerInputHandler instance;
        private ButtonControl[] allMouseButtons;

        float lastMouseMoveTime;
        Vector2 lastMousePosition;

        public virtual void Awake()
        {
            instance = this;
            allMouseButtons = new ButtonControl[] { Mouse.current.leftButton, Mouse.current.rightButton, Mouse.current.middleButton, Mouse.current.backButton, Mouse.current.forwardButton };
        }

        public virtual void Update()
        {
            if (!Player.instance.identity) return;

            if (lastMousePosition != Mouse.current.position.ReadValue())
            {
                Cursor.visible = true;
                lastMousePosition = Mouse.current.position.ReadValue();
                lastMouseMoveTime = Time.realtimeSinceStartup;
            }

            if (Time.realtimeSinceStartup - lastMouseMoveTime > 1)
            {
                Cursor.visible = false;
            }

            CollectInputCommands();
            if (HasInput)
            {
                TimeManager.instance.Interrupt(Player.instance.identity.GetComponent<Tickable>());
            }
        }

        protected virtual void CollectInputCommands()
        {
            foreach (KeyControl key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    Command command = new Command { key = key.keyCode };
                    commandQueue.Enqueue(command);
                }
            }

            foreach (ButtonControl button in allMouseButtons)
            {
                if (button.wasPressedThisFrame)
                {
                    Command command = new Command
                    {
                        mouseButton = button,
                        target = Mouse.current.position.ReadValue()
                    };
                    commandQueue.Enqueue(command);
                }
            }
        }
    }
}