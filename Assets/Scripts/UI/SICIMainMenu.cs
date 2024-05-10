using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class SICIMainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text queueStatusText;
    [SerializeField] private TMP_Text queueTimerText;
    [SerializeField] private TMP_Text findMatchButtonText;    
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private Toggle isPrivateToggle;
    [SerializeField] private Slider maxPlayersSlider;

    private bool isMatchmaking;
    private bool isCancelling;
    private bool isBusy;
    private float timeInQueue;

    private void Start()
    {
        // Don't do anything if we're not a client (aka a server)
        if (ClientSingleton.Instance == null) return;

        // Reset cursor to how cursor normally looks
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        queueStatusText.text = string.Empty;
        queueTimerText.text = string.Empty;
    }

    private void Update()
    {
        if (isMatchmaking)
        {
            timeInQueue += Time.deltaTime;
            TimeSpan ts = TimeSpan.FromSeconds(timeInQueue);
            queueTimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }
    }

    public async void FindMatchButtonPressed()
    {
        if (isCancelling) return;

        if (isMatchmaking)
        {
            // Cancel matchmaking and return
            queueStatusText.text = "Cancelling...";
            isCancelling = true;

            // Make API call to cancel matchmaking
            await ClientSingleton.Instance.GameManager.CancelMatchmaking();

            isCancelling = false;
            isMatchmaking = false;
            isBusy = false;
            findMatchButtonText.text = "Find Match";
            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
            return;
        }

        if (isBusy) return;

        // Start queue
        ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);

        findMatchButtonText.text = "Cancel";
        queueStatusText.text = "Searching...";
        timeInQueue = 0f;
        isMatchmaking = true;

        isBusy = true;
    }

    private void OnMatchMade(MatchmakerPollingResult result)
    {
        // Match has been made, now update UI
        switch (result)
        {
            case MatchmakerPollingResult.Success:
                queueStatusText.text = "Connecting...";
                Debug.Log("Player is connecting via matchmaker...");
                break;
            case MatchmakerPollingResult.TicketCreationError:
                Debug.LogWarning("Ticket Creation Error :(");
                break;
            case MatchmakerPollingResult.TicketCancellationError:
                Debug.LogWarning("Ticket Cancellation Error :(");
                break;
            case MatchmakerPollingResult.TicketRetrievalError:
                Debug.LogWarning("Ticket Retrieval Error :(");
                break;
            case MatchmakerPollingResult.MatchAssignmentError:
                Debug.LogWarning("Match Assignment Error :(");
                break;
        }
    }

    public async void StartHost()
    {
        if (isBusy) return;

        isBusy = true;
        await HostSingleton.Instance.GameManager.StartHostAsync((int)maxPlayersSlider.value, isPrivateToggle.isOn);
        isBusy = false;
    }

    public async void StartClient()
    {
        if (isBusy) return;

        isBusy = true;
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
        isBusy = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isBusy) return;
        isBusy = true;

        try
        {
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }

        isBusy = false;
    }
}
