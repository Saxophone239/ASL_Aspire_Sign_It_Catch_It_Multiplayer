using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameObjects this script is attached to will only show if on a handheld device (ex. iPad or Android phone)
/// </summary>
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
