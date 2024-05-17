using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SICIRespawnHandler : NetworkBehaviour
{
    [SerializeField] private BasketPlayer playerPrefab;
    [SerializeField] private float keptCoinPercentage;

    private int keptCoins;
    private BasketPlayer player;
    private ulong playerOwnerClientId;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Handle if players exist before scene change
        BasketPlayer[] players = FindObjectsOfType<BasketPlayer>();
        foreach (BasketPlayer player in players)
        {
            HandlePlayerSpawned(player);
        }

        BasketPlayer.OnPlayerPrefabSpawned += HandlePlayerSpawned;
        BasketPlayer.OnPlayerPrefabDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        BasketPlayer.OnPlayerPrefabSpawned -= HandlePlayerSpawned;
        BasketPlayer.OnPlayerPrefabDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(BasketPlayer player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(BasketPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(BasketPlayer player)
    {
        // Get rid of coins
        keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));

        // Destroy player prefab and spawn new one
        //this.player = player;
        playerOwnerClientId = player.OwnerClientId;
        Destroy(player.gameObject);

        // Show respawn UI
        SICIGameManager.Instance.SetGameEnd(GameOverReason.Death);
        //player.NotifyGameOverClientRpc(GameOverReason.Death, player.m_OwnerRPCParams);
        RespawnPlayer();
    }

    public void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayer(playerOwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        // Wait 1 frame
        yield return null;

        // Create new player object, then assign ownerClientId so correct player controls this
        BasketPlayer playerInstance = Instantiate(playerPrefab, SICISpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        playerInstance.Wallet.TotalCoins.Value += keptCoins;
    }

    public void DestroyAllPlayers()
    {
        BasketPlayer[] players = FindObjectsOfType<BasketPlayer>();
        foreach (BasketPlayer player in players)
        {
            Destroy(player.gameObject);
        }
    }

    public void RespawnPlayerClientRequest()
    {
        ulong ownerClientId = OwnerClientId;
        RespawnPlayerServerRpc(ownerClientId, keptCoins);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RespawnPlayerServerRpc(ulong ownerClientId, int keptCoins, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            Unity.Netcode.NetworkClient client = NetworkManager.ConnectedClients[clientId];

            // Do things for this client
            StartCoroutine(RespawnPlayer(ownerClientId, keptCoins));
        }
    }

    //****************************//
    /**
    *
    * Below logic is testing how to send a request to the server
    * and broadcast a panel to either the player or everyone
    *
    */
    //****************************//

    [SerializeField] private GameObject broadcastPanel;

    public void DisplayToJustPlayer()
    {
        DisplaySomethingServerRpc(false);
    }

    public void DisplayToEveryPlayer()
    {
        DisplaySomethingServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisplaySomethingServerRpc(bool sendToEveryone, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            Unity.Netcode.NetworkClient client = NetworkManager.ConnectedClients[clientId];

            // Do things for this client
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = null
                }
            };

            if (sendToEveryone)
            {
                clientRpcParams.Send.TargetClientIds = NetworkManager.ConnectedClientsIds;
            }
            else
            {
                clientRpcParams.Send.TargetClientIds = new ulong[] { clientId };
            }

            DisplaySomethingToPlayersClientRpc(clientRpcParams);
        }
    }

    [ClientRpc]
    public void DisplaySomethingToPlayersClientRpc(ClientRpcParams clientRpcParams)
    {
        DisplayPanel();
    }

    public void DisplayPanel()
    {
        broadcastPanel.SetActive(true);
    }
}
