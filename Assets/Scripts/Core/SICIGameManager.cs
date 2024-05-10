using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class SICIGameManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private SICIRespawnHandler respawnHandler;
    [SerializeField] private SICISpawner spawner;
    
    [Header("UI Settings")]
    [SerializeField] private TMP_Text gameTimerText;
    [SerializeField] private GameObject respawnPanel;
    [SerializeField] private TMP_Text respawnPanelText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;

    [SerializeField]
    [Tooltip("Time Remaining until the game starts")]
    private float m_delayedStartTime = 15.0f;

    // These help to simplify checking server vs client
    private bool m_isClientGameOver;
    private bool m_isClientGameStarted;
    private bool m_isClientStartCountdown;

    private NetworkVariable<bool> m_isCountdownStarted = new NetworkVariable<bool>();

    // The timer should only be synced at the beginning and then let the client to update it in a predictive manner
    private bool m_isReplicatedTimeSent;
    private float m_timeRemaining;

    private static SICIGameManager instance;
    public static SICIGameManager Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType<SICIGameManager>();
            if (instance == null)
            {
                Debug.LogError("No SICIGameManager in the scene!");
                return null;
            }

            OnSingletonReady?.Invoke();
            return instance;
        }
    }
    public static event Action OnSingletonReady;

    public NetworkVariable<bool> IsGameStarted = new NetworkVariable<bool>();
    public NetworkVariable<bool> IsGameOver = new NetworkVariable<bool>();

    private void Awake()
    {
        if (IsServer)
        {
            IsGameOver.Value = false;
            IsGameStarted.Value = true;

            // Set our time remaining locally
            m_timeRemaining = m_delayedStartTime;

            // Set for server side
            Debug.Log("setting replicated time sent false");
            m_isReplicatedTimeSent = false;
        }
        else
        {
            // We do a check for the client side value upon instantiating the class (should be zero)
            Debug.LogFormat("Client side we started with a timer value of {0}", m_timeRemaining);
        }
    }

    private void Update()
    {
        // Is the game over?
        if (IsCurrentGameOver()) return;

        // Update game timer (if the game hasn't started)
        UpdateGameTimer();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            m_isClientGameOver = IsGameOver.Value;
            m_isClientStartCountdown = m_isCountdownStarted.Value;
            m_isClientGameStarted = IsGameStarted.Value;

            m_isCountdownStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_isClientStartCountdown = newValue;
                Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
            };

            IsGameStarted.OnValueChanged += (oldValue, newValue) =>
            {
                m_isClientGameStarted = newValue;
                gameTimerText.gameObject.SetActive(!m_isClientGameStarted);
                Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
            };

            IsGameOver.OnValueChanged += (oldValue, newValue) =>
            {
                m_isClientGameOver = newValue;
                Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
            };
        }

        // Both client and host/server will set the scene state to "ingame" which makes the players visible and allows for the players to be controlled
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"client has connected with id: {clientId}");
        if (m_isReplicatedTimeSent)
        {
            // Send the RPC only to the newly connected client
            Debug.Log("Sending replicated time via client RPC");
            SetReplicatedTimeRemainingClientRPC(m_timeRemaining, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new List<ulong>()
                    {
                        clientId
                    }
                }
            });
        }
    }

    private bool HasGameStarted()
    {
        if (IsServer)
        {
            return IsGameStarted.Value;
        }
        return m_isClientGameStarted;
    }

    private bool IsCurrentGameOver()
    {
        if (IsServer)
        {
            return IsGameOver.Value;
        }
        return m_isClientGameOver;
    }

    private bool ShouldStartCountDown()
    {
        // If the game has ended, then don't bother with the rest of the count down checks
        if (IsCurrentGameOver()) return false;
        
        if (IsServer)
        {
            // Check if all clients are loaded
            m_isCountdownStarted.Value = true;
            
            // While we are counting down, continually set the replicated time remaining value for clients (client should only receive the update once)
            if (m_isCountdownStarted.Value && !m_isReplicatedTimeSent)
            {
                SetReplicatedTimeRemainingClientRPC(m_delayedStartTime);
                Debug.Log("setting replicated time sent true");
                m_isReplicatedTimeSent = true;
            }
            
            return m_isCountdownStarted.Value;
        }
        
        return m_isClientStartCountdown;
    }

    [ClientRpc]
    private void SetReplicatedTimeRemainingClientRPC(float delayedStartTime, ClientRpcParams clientRpcParams = new ClientRpcParams())
    {
        // See the ShouldStartCountDown method for when the server updates the value
        if (m_timeRemaining == 0)
        {
            Debug.LogFormat("Client side our first timer update value is {0}", delayedStartTime);
            m_timeRemaining = delayedStartTime;
        }
        else
        {
            Debug.LogFormat("Client side we got an update for a timer value of {0} when we shouldn't", delayedStartTime);
        }
    }

    private void UpdateGameTimer()
    {
        if (!ShouldStartCountDown()) return;
        
        if (!IsCurrentGameOver() && m_timeRemaining > 0.0f)
        {
            m_timeRemaining -= Time.deltaTime;
            
            if (IsServer && m_timeRemaining <= 0.0f) // Only the server should be updating this
            {
                m_timeRemaining = 0.0f;
                IsGameOver.Value = true;
                OnTimerEnded();
            }

            if (m_timeRemaining > 0.1f)
            {
                int minutes = Mathf.FloorToInt(m_timeRemaining / 60F);
                int seconds = Mathf.FloorToInt(m_timeRemaining - minutes * 60);

                string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
                //gameTimerText.SetText(niceTime);
                gameTimerText.text = niceTime;
            }
        }
    }

    private void OnTimerEnded()
    {
        //gameTimerText.gameObject.SetActive(false);

        SetGameEnd(GameOverReason.TimerOver);
    }

    public void DisplayRespawnPanel(string message)
    {
        if (respawnPanelText && respawnPanel)
        {
            respawnPanelText.SetText(message);
            respawnPanelText.gameObject.SetActive(true);
            respawnPanel.gameObject.SetActive(true);
        }
    }

    public void DisplayGameOverPanel(string message)
    {
        if (gameOverText && gameOverPanel)
        {
            gameOverText.SetText(message);
            gameOverText.gameObject.SetActive(true);
            gameOverPanel.gameObject.SetActive(true);
        }
    }

    public void SetGameEnd(GameOverReason reason)
    {
        Assert.IsTrue(IsServer, "SetGameEnd should only be called server side!");

        // If game ended due to timer ending, end game for all players
        if (reason == GameOverReason.TimerOver)
        {
            this.IsGameOver.Value = true;
            //respawnHandler.DestroyAllPlayers();
            spawner.StopSpawningWords();
            BroadcastGameOverClientRpc(reason); // Notify our clients!
            return;
        }

        // Game probably ended due to one player dying
        return;
    }

    [ClientRpc]
    public void BroadcastGameOverClientRpc(GameOverReason reason)
    {
        var localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        Assert.IsNotNull(localPlayerObject);

        if (localPlayerObject.TryGetComponent<BasketPlayer>(out var player))
            player.NotifyGameOver(reason);
    }
}

public enum GameOverReason : byte
{
    None = 0,
    TimerOver = 1,
    Death = 2,
    Max,
}
