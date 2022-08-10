using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CommandQueue : LinkedList<Command>
{

    public void AddIfNotExists(KeyCode k, bool autoRemove = false)
    {
        foreach (var c in this) if (c.key == k && c.hasExecuted == false && c.shouldRemove == false) return;

        Command command = new Command(k, autoRemove);
        AddLast(command);
    }

    public void RemoveIfExecuted(KeyCode k)
    {
        var node = First;
        while(node != null)
        {
            var nextNode = node.Next;
            if (node.Value.key == k)
            {
                if (node.Value.hasExecuted) Remove(node);
                else node.Value.shouldRemove = true;
            }
           
            node = nextNode;
        }
    }
}
