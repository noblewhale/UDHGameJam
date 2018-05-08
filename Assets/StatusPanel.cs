using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusPanel : MonoBehaviour
{
    Camera cam;
    public float panelHeight = 3;

	void Start ()
    {
        cam = GetComponentInParent<Camera>();
	}
	
	void Update ()
    {
        transform.localPosition = new Vector3(0, -cam.orthographicSize + panelHeight, transform.localPosition.z);
	}
}
