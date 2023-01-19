#if UNITY_EDITOR
namespace Noble.TileEngine
{
    using UnityEditor;

    [CustomEditor(typeof(Map), true)]
    public class MapEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Map myObject = (Map)target;
            EditorGUILayout.LabelField("Calculated Area: " + myObject.totalArea);
            EditorGUILayout.Separator();
            DrawDefaultInspector();
        }
    }
}
#endif