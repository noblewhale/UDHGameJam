using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    Player owner;

    public float rotationMaxSpeed = .01f;
    public float rotationLerpFactor = .5f;
    public float movementMaxSpeed = .1f;
    public float movementLerpFactor = .5f;
    public int cameraOffset = 3;
    public float rotation;

    void Start ()
    {
        owner = FindObjectOfType<Player>();
	}
	
	void Update ()
    {
        if (!owner || !owner.identity) return;

        float percentOfWidth = (owner.identity.transform.localPosition.x + owner.map.tileWidth / 2) / owner.map.TotalWidth;
        float targetRotation = 2 * Mathf.PI * (1 - percentOfWidth) - Mathf.PI / 2;
        if (targetRotation - rotation > Mathf.PI)
        {
            targetRotation = targetRotation - (2 * Mathf.PI);
        }
        targetRotation = Mathf.Lerp(rotation, targetRotation, rotationLerpFactor);
        float relativeRotation = targetRotation - rotation;
        if (Mathf.Abs(relativeRotation) > rotationMaxSpeed)
        {
            relativeRotation = Mathf.Sign(relativeRotation) * rotationMaxSpeed;
        }
        rotation += relativeRotation;
        owner.map.polarWarpMaterial.SetFloat("_Rotation", rotation);
        if (owner.isControllingCamera)
        {
            //Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y + cameraOffset, Camera.main.transform.position.z);
            Vector2 targetPos = new Vector2(Camera.main.transform.position.x, owner.identity.transform.position.y + cameraOffset);
            targetPos = Vector2.Lerp(Camera.main.transform.position, targetPos, movementLerpFactor);
            Vector2 relativePos = targetPos - (Vector2)Camera.main.transform.position;

            if (relativePos.magnitude > movementMaxSpeed)
            {
                relativePos = relativePos.normalized * movementMaxSpeed;
            }

            targetPos = Camera.main.transform.position + (Vector3)relativePos;
            Camera.main.transform.position = new Vector3(targetPos.x, targetPos.y, Camera.main.transform.position.z);
        }
    }
}
