using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SICITimer : NetworkBehaviour
{
    private bool isClientGameStarted;

    public NetworkVariable<bool> IsGameStarted = new NetworkVariable<bool>();

    private bool HasGameStarted()
    {
        if (IsServer)
        {
            return IsGameStarted.Value;
        }
        return isClientGameStarted;
    }

    // private bool ShouldStartCountDown()
    // {
    //     // If the game has started, then don't bother with the rest of the count down checks
    //     if (HasGameStarted()) return false;
    // }
}
