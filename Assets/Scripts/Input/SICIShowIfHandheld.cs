using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SICIShowIfHandheld : MonoBehaviour
{
    private void Awake()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
