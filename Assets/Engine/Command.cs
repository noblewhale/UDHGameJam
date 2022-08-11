using UnityEngine;

public class Command
{
    public KeyCode key;
    public Vector3 target;

    public override bool Equals(object obj)
    {
        var other = (Command)obj;
        return this.key == other.key && target == other.target;
    }

    public override int GetHashCode()
    {
        var hashCode = 1464462768;
        hashCode = hashCode * -1521134295 + key.GetHashCode();
        hashCode = hashCode * -1521134295 + target.GetHashCode();

        return hashCode;
    }
}
