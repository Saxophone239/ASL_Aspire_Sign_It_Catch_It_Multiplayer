using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Collector : MonoBehaviour
{
    // This script deletes any words that pass the basket
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
        {
            networkObject.Despawn();
        }
        Destroy(collision.gameObject);
    }
}
