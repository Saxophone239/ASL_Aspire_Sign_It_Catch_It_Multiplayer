using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable
{
    private NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // Check that player that left isn't host, who has clientId = 0
        if (clientId != 0 && clientId != networkManager.LocalClientId) return;

        Disconnect();
    }

    public void Disconnect()
    {
        // Load main menu screen
        if (SceneManager.GetActiveScene().name != BasketGameScenes.MenuSceneName)
        {
            SceneManager.LoadScene(BasketGameScenes.MenuSceneName);
        }

        // If we've timed out, then manually sever connection as client.
        if (networkManager.IsConnectedClient)
        {
            networkManager.Shutdown();
        }
    }

    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}
