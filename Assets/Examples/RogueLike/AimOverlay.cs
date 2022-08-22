using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimOverlay : MonoBehaviour
{
    public static AimOverlay instance;
    void Start()
    {
        instance = this;
        gameObject.SetActive(false);
    }
}
