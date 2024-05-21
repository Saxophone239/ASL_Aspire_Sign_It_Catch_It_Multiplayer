using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{
    private string serverIP;
    private int serverPort;
    private int queryPort;
    private MatchplayBackfiller backfiller;

    #if !UNITY_IOS
    private MultiplayAllocationService multiplayAllocationService;
    #endif

    public NetworkServer NetworkServer { get; private set; }

    public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager)
    {
        this.serverIP = serverIP;
        this.serverPort = serverPort;
        this.queryPort = queryPort;
        NetworkServer = new NetworkServer(manager);
        #if !UNITY_IOS
        multiplayAllocationService = new MultiplayAllocationService();
        #endif
    }

    public async Task StartGameServerAsync()
    {
        #if !UNITY_IOS
        // Begin loop to check server status every 1 second
        await multiplayAllocationService.BeginServerCheck();
        #endif

        // Server has spun up since match has been made, read data from match data
        try
        {
            MatchmakingResults matchmakerPayload = await GetMatchmakerPayload();

            if (matchmakerPayload != null)
            {
                // Start backfilling (process to get more players in server once it starts)
                await StartBackfill(matchmakerPayload);
                NetworkServer.OnUserJoined += UserJoined;
                NetworkServer.OnUserLeft += UserLeft;
            }
            else
            {
                Debug.LogWarning("Matchmaker payload timed out");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

        // Try to open server connection
        if (!NetworkServer.OpenConnection(serverIP, serverPort))
        {
            Debug.LogWarning("NetworkServer did not start as expected.");
            return;
        }
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload()
    {
        #if !UNITY_IOS
        // Subscribe to events in backend and return data
        Task<MatchmakingResults> matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

        // Run task and move on whether task finishes or it times out in 60 seconds
        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(60000)) == matchmakerPayloadTask)
        {
            Debug.Log("Getting matchmaker payload success!");
            return matchmakerPayloadTask.Result;
        }

        // We timed out
        return null;
        #endif
        
        await Task.Delay(1);
        return null;
    }

    private async Task StartBackfill(MatchmakingResults payload)
    {
        backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}", payload.QueueName, payload.MatchProperties, 4);

        if (backfiller.NeedsPlayers())
        {
            await backfiller.BeginBackfilling();
        }
    }

    private void UserJoined(UserData user)
    {
        backfiller.AddPlayerToMatch(user);
        #if !UNITY_IOS
        multiplayAllocationService.AddPlayer();
        #endif

        if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
        {
            // We have reached max players, stop backfilling
            _ = backfiller.StopBackfill();
        }
    }

    private void UserLeft(UserData user)
    {
        int playerCount = backfiller.RemovePlayerFromMatch(user.userAuthId);
        #if !UNITY_IOS
        multiplayAllocationService.RemovePlayer();
        #endif

        if (playerCount <= 0)
        {
            // Server is empty, close server
            CloseServer();
            return;
        }

        if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
        {
            // We need more players, begin backfilling again
            _ = backfiller.BeginBackfilling();
        }
    }

    private async void CloseServer()
    {
        await backfiller.StopBackfill();
        Dispose();
        Application.Quit();
    }

    public void Dispose()
    {
        NetworkServer.OnUserJoined -= UserJoined;
        NetworkServer.OnUserLeft -= UserLeft;

        backfiller?.Dispose();
        #if !UNITY_IOS
        multiplayAllocationService?.Dispose();
        #endif
        NetworkServer?.Dispose();
    }
}