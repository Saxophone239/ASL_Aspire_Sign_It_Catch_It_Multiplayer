using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform  // extend this instead of MonoBehavior
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn(); // Do what normally happens in OnNetworkSpawn()
        CanCommitToTransform = IsOwner; // Check value of IsOwner, whether client has control over transform depends on that
    }

    protected override void Update()
    {
        CanCommitToTransform = IsOwner; // Check value of IsOwner, whether client has control over transform depends on that
        base.Update(); // Do what normally happens in Update()

        // Safety checks
        if (NetworkManager != null)
        {
            if (NetworkManager.IsConnectedClient || NetworkManager.IsListening)
            {
                if (CanCommitToTransform) // "if client has permission to change transform:"
                {
                    // Take client's changed transform and send it to the server with a specified time
                    TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                }
            }
        }
    }

    protected override bool OnIsServerAuthoritative()
    {
        // Tell servier it no longer has full authorization, instead client does
        // (anything client does will be trusted)
        return false;
    }
}
