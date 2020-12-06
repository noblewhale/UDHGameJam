using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositionFromEdge : MonoBehaviour
{
    public float vertical;
    public float horizontal;
    void Awake()
    {
        float maxWidth = Camera.main.orthographicSize * 2 * Camera.main.aspect - Camera.main.orthographicSize * 2 * .4f;
        transform.localScale = new Vector3(maxWidth, maxWidth, 1);
        float x, y;
        if (horizontal < 0)
        {
            x = -transform.localScale.x / 2 + Camera.main.orthographicSize * Camera.main.aspect + horizontal;
        }
        else
        {
            x = transform.localScale.x / 2 - Camera.main.orthographicSize * Camera.main.aspect + horizontal;
        }
        if (vertical < 0)
        {
            y = -transform.localScale.y / 2 + Camera.main.orthographicSize + vertical;
        }
        else
        {
            y = transform.localScale.y / 2 - Camera.main.orthographicSize + vertical;
        }
        transform.localPosition = new Vector3(x, y, transform.localPosition.z);
    }
}
