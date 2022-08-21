using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeAnimationOffset : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Animator>().SetFloat("CycleOffset", Random.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
