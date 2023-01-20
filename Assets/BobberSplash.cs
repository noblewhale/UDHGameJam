using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberSplash : MonoBehaviour
{
    public FishingBehaviour fishingParameter;

    public void OnBob() 
    {
        
        fishingParameter.bobCount++;
        fishingParameter.didBob = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
