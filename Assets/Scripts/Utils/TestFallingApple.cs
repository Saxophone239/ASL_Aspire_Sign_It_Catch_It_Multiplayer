using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestFallingApple : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        Debug.Log($"I touched a player's collider named {other.gameObject.name}");

        //gameObject.GetComponent<NetworkObject>().Despawn(); // Just this line removes client-side detection, I'm guessing it's because the server destroys the object before the client can read it
        //Destroy(gameObject);
    }
}
