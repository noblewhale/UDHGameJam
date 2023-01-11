#if UNITY_EDITOR
namespace Noble.TileEngine
{
    using System.Reflection;
    using UnityEditor;
    using UnityEngine.Events;
    using static Noble.TileEngine.DungeonObject;

    [CustomEditor(typeof(DungeonObject))]
    public class DungeonObjectEditor : Editor
    {
        bool showEvents = false;
        public override void OnInspectorGUI()
        {
            var myObject = (DungeonObject)target;
            
            DrawDefaultInspector();

            showEvents = EditorGUILayout.Foldout(showEvents, "Events");
            if (showEvents)
            {
                var fields = typeof(DungeonObject).GetFields();
                foreach (var field in fields)
                {
                    if (field.FieldType.Name.Contains("DungeonObjectEvent"))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                    }
                }
            }
        }
    }
}
#endif