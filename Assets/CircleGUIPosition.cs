using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CircleGUIPosition : MonoBehaviour
{
    public float distance = 0;
    public int divisions = 360;
    public MeshRenderer mapRenderer;
    public bool rotate = false;
    RectTransform rect;
    Vector2 pos;
    Vector2 center;
    float radius;
    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        var parentRect = rect.parent.GetComponent<RectTransform>();
        center = new Vector2(0, 0);
        center.x += (mapRenderer.transform.position.x / (Camera.main.orthographicSize * 2 * 2340/1080.0f)) * 2340;
        center.y += (mapRenderer.transform.position.y / (Camera.main.orthographicSize * 2)) * 1080;
        radius = (mapRenderer.transform.localScale.y / (Camera.main.orthographicSize * 2)) * 1080 / 2;
    }

    // Update is called once per frame
    void Update()
    {
        float angle = 2 * Mathf.PI * distance / (2 * Mathf.PI * radius) + Mathf.PI;
        float x = center.x + radius * Mathf.Sin(angle);
        float y = center.y + radius * Mathf.Cos(angle);
        pos = new Vector2(x, y);
        if (rotate)
        {
            rect.transform.localRotation = Quaternion.Euler(0, 0, -Mathf.Rad2Deg * angle);
        }
        rect.transform.localPosition = pos;
    }
}
