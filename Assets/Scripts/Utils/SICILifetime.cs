using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SICILifetime : MonoBehaviour
{
    [SerializeField] private float lifetime = 10.0f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
