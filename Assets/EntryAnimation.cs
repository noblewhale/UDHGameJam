using System;
using UnityEngine;

public class EntryAnimation : MonoBehaviour
{
    Character character;
    public event Action OnDoneAnimating;
    public bool isAnimating = false;

    void Start()
    {
        character = FindObjectOfType<Character>();
    }

    void Update ()
    {
        if (isAnimating)
        {
            Camera.main.transform.position += Vector3.up * Time.deltaTime * 10f;
            if (Camera.main.transform.position.y > character.transform.position.y + character.cameraOffset)
            {
                isAnimating = false;
                if (OnDoneAnimating != null) OnDoneAnimating();
            }
        }
	}
}
