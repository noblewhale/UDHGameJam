#if UNITY_EDITOR
namespace Noble.TileEngine
{
    using UnityEditor;

    [CustomEditor(typeof(Creature))]
    public class CreatureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Creature myObject = (Creature)target;
            EditorGUILayout.LabelField("Effective View Distance: " + myObject.effectiveViewDistance);
            EditorGUILayout.Separator();
            DrawDefaultInspector();
        }
    }
}
#endif