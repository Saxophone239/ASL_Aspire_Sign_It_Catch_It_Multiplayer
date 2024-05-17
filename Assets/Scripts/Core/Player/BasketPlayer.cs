using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

// General script that determines whether a GameObject is a player
public class BasketPlayer : NetworkBehaviour
{
    //[Header("References")]
    [field: SerializeField] public BasketHealth Health { get; private set; }
    [field: SerializeField] public CoinWallet Wallet { get; private set; }

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<BasketPlayer> OnPlayerPrefabSpawned;
    public static event Action<BasketPlayer> OnPlayerPrefabDespawned;

    public ClientRpcParams ThisClientRpcParams { get; private set; }

    // Booleans for powerups
    private bool isLightning = false;
    private bool isStopwatch = false;
    private bool isBurger = false;
    private bool isMultiplier = false;

    private bool m_HasGameStarted;
    private bool m_IsGameOver;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Get and display player name
            UserData userData = null;

            if (IsHost)
            {
                userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            else
            {
                // We are a dedicated server
                userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }

            // Set up username
            PlayerName.Value = userData.userName;

            // Send logic to server when player is spawned
            OnPlayerPrefabSpawned?.Invoke(this);

            // Set RPC Params
            ThisClientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };
        }

        if (IsOwner)
        {
            // Currently nothing needs to be done owner-only
        }

        if (!SICIGameManager.Instance)
            SICIGameManager.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
        else
            SubscribeToDelegatesAndUpdateValues();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            // Send logic to server when player is despawned
            OnPlayerPrefabDespawned?.Invoke(this);
        }
    }

    private void SubscribeToDelegatesAndUpdateValues()
    {
        SICIGameManager.Instance.IsGameStarted.OnValueChanged += OnGameStartedChanged;
        SICIGameManager.Instance.IsGameOver.OnValueChanged += OnGameOverChanged;

        m_HasGameStarted = SICIGameManager.Instance.IsGameStarted.Value;
    }

    private void OnGameStartedChanged(bool previousValue, bool newValue)
    {
        m_HasGameStarted = newValue;
    }

    private void OnGameOverChanged(bool previousValue, bool newValue)
    {
        m_IsGameOver = newValue;
    }

    [ClientRpc]
    public void NotifyGameOverClientRpc(GameOverReason reason, ClientRpcParams clientParams)
    {
        NotifyGameOver(reason);
    }

    /// <summary>
    /// This should only be called locally, either through NotifyGameOverClientRpc or through the InvadersGame.BroadcastGameOverReason
    /// </summary>
    /// <param name="reason"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void NotifyGameOver(GameOverReason reason)
    {
        Assert.IsTrue(IsLocalPlayer);
        m_HasGameStarted = false;
        switch (reason)
        {
            case GameOverReason.None:
                SICIGameManager.Instance.DisplayGameOverPanel("You have lost! Unkown reason!");
                break;
            case GameOverReason.TimerOver:
                SICIGameManager.Instance.DisplayGameOverPanel("You have lost! Timer is over!");
                break;
            case GameOverReason.Death:
                SICIGameManager.Instance.DisplayRespawnPanel("You have died! Click to respawn.");
                break;
            case GameOverReason.Max:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
        }
    }
}
