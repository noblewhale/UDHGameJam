namespace Noble.TileEngine
{
    using UnityEditor;

    [CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Tile tile = (Tile)target;
            foreach (var ob in tile.objectList)
            {
                EditorGUILayout.LabelField("Object: " + ob.name + "[" + ob.GetType() + "]");
            }
            EditorGUILayout.Separator();
            DrawDefaultInspector();
        }
    }
}