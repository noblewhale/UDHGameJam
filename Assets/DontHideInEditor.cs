using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DontHideInEditor : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        gameObject.hideFlags = HideFlags.None;
#if UNITY_EDITOR
        try { EditorApplication.DirtyHierarchyWindowSorting(); } catch { }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
