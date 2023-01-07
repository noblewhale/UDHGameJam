using Noble.TileEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoHandToggle : MonoBehaviour
{
    public GameObject singleHandObject;
    public GameObject twoHandObject;


    private void OnEnable()
    {
        UpdateState();

        if (Player.instance && Player.instance.identity)
        {
            Player.instance.identity.Equipment.onChange += UpdateState;
        }
    }

    private void OnDisable()
    {
        if (Player.instance && Player.instance.identity)
        {
            Player.instance.identity.Equipment.onChange -= UpdateState;
        }
    }

    void UpdateState()
    {
        if (Player.instance.identity.Equipment.GetEquipment(Equipment.Slot.TWO_HANDED))
        {
            if (twoHandObject)
            {
                twoHandObject.SetActive(true);
                var equipable = twoHandObject.GetComponentInParent<Equipable>();
                if (equipable.IsEquipped)
                {
                    if (twoHandObject.transform.Find("Handle"))
                    {
                        equipable.handle = twoHandObject.transform.Find("Handle").transform;
                        equipable.UpdatePosition();
                    }
                }
            }
            if (singleHandObject)
            {
                singleHandObject.SetActive(false);
            }
        }
        else
        {
            if (twoHandObject)
            {
                twoHandObject.SetActive(false);
            }
            if (singleHandObject)
            {
                singleHandObject.SetActive(true);
                var equipable = singleHandObject.GetComponentInParent<Equipable>();
                if (equipable.IsEquipped)
                {
                    if (singleHandObject.transform.Find("Handle"))
                    {
                        equipable.handle = singleHandObject.transform.Find("Handle").transform;
                        equipable.UpdatePosition();
                    }
                }
            }
        }
    }
}
