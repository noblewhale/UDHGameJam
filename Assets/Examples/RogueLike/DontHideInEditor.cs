namespace Noble.DungeonCrawler
{
    using UnityEditor;
    using UnityEngine;

    [ExecuteInEditMode]
    public class DontHideInEditor : MonoBehaviour
    {
        void Awake()
        {
            gameObject.hideFlags = HideFlags.None;
#if UNITY_EDITOR
            try { EditorApplication.DirtyHierarchyWindowSorting(); } catch { }
#endif
        }
    }
}