using UnityEngine;

public class Command
{
    public KeyCode key;
    public bool hasExecuted;
    public bool shouldRemove;

    public Command(KeyCode key)
    {
        this.key = key;
    }

    public override bool Equals(object obj)
    {
        var other = (Command)obj;
        return this.key == other.key && this.hasExecuted == other.hasExecuted && this.shouldRemove == other.shouldRemove;
    }

    public override int GetHashCode()
    {
        var hashCode = 1464462768;
        hashCode = hashCode * -1521134295 + key.GetHashCode();
        hashCode = hashCode * -1521134295 + hasExecuted.GetHashCode();
        hashCode = hashCode * -1521134295 + shouldRemove.GetHashCode();
        return hashCode;
    }
}
