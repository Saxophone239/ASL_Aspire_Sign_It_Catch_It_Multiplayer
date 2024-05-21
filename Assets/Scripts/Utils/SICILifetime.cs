using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Any gameobjects with this script attached will destroy itself after a set amount of time.
/// </summary>
public class SICILifetime : MonoBehaviour
{
    [SerializeField] private float lifetime = 10.0f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
