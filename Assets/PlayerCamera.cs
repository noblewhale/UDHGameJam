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

        SetRotation(owner.identity.x, owner.identity.y, rotationLerpFactor, rotationMaxSpeed);
        if (owner.isControllingCamera)
        {
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

    public void SetRotation(int x, int y, float lerpFactor, float maxSpeed)
    {
        float percentOfWidth = (float) x / owner.map.width;
        float targetRotation = 2 * Mathf.PI * (1 - percentOfWidth) - Mathf.PI / 2;
        if (targetRotation - rotation > Mathf.PI)
        {
            targetRotation = targetRotation - (2 * Mathf.PI);
        }
        targetRotation = Mathf.Lerp(rotation, targetRotation, lerpFactor);
        float relativeRotation = targetRotation - rotation;
        if (Mathf.Abs(relativeRotation) > maxSpeed)
        {
            relativeRotation = Mathf.Sign(relativeRotation) * maxSpeed;
        }
        rotation += relativeRotation;
        owner.map.polarWarpMaterial.SetFloat("_Rotation", rotation);
    }
}
