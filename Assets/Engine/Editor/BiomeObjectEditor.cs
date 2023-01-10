#if UNITY_EDITOR
namespace Noble.TileEngine
{
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    [CustomEditor(typeof(BiomeObject))]
    public class BiomeObjectEditor : Editor
    {
        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();
        public Bounds tempBounds;
        private bool firstTime = true;
        Vector3 previousPos;
        RectIntExclusive previousArea;
        Map map;

        private void Awake()
        {
            map = FindObjectOfType<Map>();
            firstTime = true;
        }

        private void OnEnable()
        {
            firstTime = true;
        }

        void OnSceneGUI()
        {
            BiomeObject myObject = (BiomeObject)target;

            if (firstTime || previousPos != myObject.transform.position || !myObject.area.Equals(previousArea))
            {
                // copy the target object's data to the handle
                Vector2 areaCenter = new Vector2(myObject.area.xMin + myObject.area.xMax, myObject.area.yMin + myObject.area.yMax);
                areaCenter /= 2.0f;
                tempBounds.center = myObject.transform.position;
                tempBounds.center += (Vector3)areaCenter;
                tempBounds.center += (Vector3)map.tileDimensions / 2;
                tempBounds.size = new Vector2(myObject.area.width, myObject.area.height);
                firstTime = false;
            }
            previousPos = myObject.transform.position;
            previousArea = myObject.area;

            // copy the target object's data to the handle
            m_BoundsHandle.center = tempBounds.center;
            m_BoundsHandle.size = tempBounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RegisterCompleteObjectUndo(new Object[] { myObject, this }, "Change Bounds");
                //Undo.RecordObject(myObject, "Change Bounds");

                tempBounds.center = m_BoundsHandle.center;
                tempBounds.size = m_BoundsHandle.size;

                // copy the handle's updated data back to the target object
                float width = tempBounds.size.x;
                float height = tempBounds.size.y;
                int minX = Mathf.RoundToInt(tempBounds.center.x - width / 2) - Mathf.FloorToInt(myObject.transform.position.x);
                int maxX = Mathf.RoundToInt(tempBounds.center.x + width / 2) - Mathf.FloorToInt(myObject.transform.position.x) - 1;
                int minY = Mathf.RoundToInt(tempBounds.center.y - height / 2) - Mathf.FloorToInt(myObject.transform.position.y);
                int maxY = Mathf.RoundToInt(tempBounds.center.y + height / 2) - Mathf.FloorToInt(myObject.transform.position.y) - 1;
                RectIntExclusive newBounds = new RectIntExclusive();
                newBounds.SetMinMax(minX, maxX, minY, maxY);
                myObject.area = newBounds;
            }
        }
    }
}
#endif