using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    public void CollectWord(FallingWord word, bool isCorrect)
    {
        int coinValue;
        if (isCorrect)
        {
            coinValue = word.Collect();
        }
        else
        {
            coinValue = word.CollectIncorrect();
        }
        
        if (!IsServer) return;
        //Debug.Log($"Before changing TotalCoins adding {coinValue}");
        TotalCoins.Value += coinValue;
        //Debug.Log($"After changing TotalCoins adding {coinValue}");
    }
}
