using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinueButton : MonoBehaviour
{
    public GameObject window;

    public void Continue()
    {
        window.SetActive(false);
        PlayerCamera.instance.owner = Player.instance.identity;
        Player.instance.playerInput.enabled = true;
    }
}
