using System;
using UnityEngine;

public class EntryAnimation : MonoBehaviour
{
    Player player;
    public event Action OnDoneAnimating;
    public bool isAnimating = false;
    PlayerCamera playerCamera;

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerCamera = GetComponent<PlayerCamera>();
    }

    void Update ()
    {
        if (isAnimating && player && player.identity)
        {
            Camera.main.transform.position += Vector3.up * Time.deltaTime * 10f;
            if (Camera.main.transform.position.y > player.identity.transform.position.y + playerCamera.cameraOffset)
            {
                isAnimating = false;
                if (OnDoneAnimating != null) OnDoneAnimating();
            }
        }
	}
}
