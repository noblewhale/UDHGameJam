using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyOrthoSize : MonoBehaviour
{

    public Camera copyFrom;

	// Update is called once per frame
	void Update ()
    {
        GetComponent<Camera>().orthographicSize = copyFrom.orthographicSize;
    }
}
