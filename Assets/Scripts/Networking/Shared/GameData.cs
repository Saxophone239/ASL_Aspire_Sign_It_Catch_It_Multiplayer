using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// TODO: implement different maps?
public enum Map
{
    Default
}

public enum GameMode
{
    Default
}

// TODO: implement games? not a priority though
public enum GameQueue
{
    Solo,
    Team // Currently not implemented
}

[Serializable]
public class UserData
{
    public string userName;
    public string userAuthId;
    public GameInfo userGamePreferences = new GameInfo();
}

[Serializable]
public class GameInfo
{
    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        return gameQueue switch
        {
            GameQueue.Solo => "solo-queue",
            GameQueue.Team => "team-queue",
            _ => "solo-queue",
        };
    }
}
