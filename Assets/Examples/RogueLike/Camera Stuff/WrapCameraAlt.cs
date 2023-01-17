using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapCameraAlt : MonoBehaviour
{
    public Camera leftCamera;
    public Camera rightCamera;
    Camera centerCamera;
    public float wrapWidth = 20;
    public float wrapCenter = 0;

    void Start()
    {
        centerCamera = GetComponent<Camera>();
        SetWrapWidth(wrapWidth, wrapCenter);
    }

    public void SetWrapWidth(float wrapWidth, float wrapCenter)
    {
        this.wrapWidth = wrapWidth;
        this.wrapCenter = wrapCenter;

        rightCamera.transform.localPosition = Vector3.right * wrapWidth;
        leftCamera.transform.localPosition = Vector3.left * wrapWidth;
    }

    private void Update()
    {
        float relativeX = transform.position.x - wrapCenter;
        float camHalfWidth = centerCamera.orthographicSize * centerCamera.aspect;
        if (relativeX - camHalfWidth < -wrapWidth / 2)
        {
            rightCamera.enabled = true;
            leftCamera.enabled = false;
        }
        else if(relativeX + camHalfWidth > wrapWidth / 2)
        {
            leftCamera.enabled = true;
            rightCamera.enabled = false;
        }
        else
        {
            leftCamera.enabled = false;
            rightCamera.enabled = false;
        }

        if (!leftCamera.enabled && !rightCamera.enabled)
        {
            centerCamera.clearFlags = CameraClearFlags.SolidColor;
        }
        else
        {
            centerCamera.clearFlags = CameraClearFlags.Nothing;
        }
    }
}
