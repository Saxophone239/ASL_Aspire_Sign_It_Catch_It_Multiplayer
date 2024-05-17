using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerColorLookup", menuName = "Player Color Lookup")]
public class SICIColorLookup : ScriptableObject
{
    [SerializeField] private Color[] playerColors;

    // Set playerIndex to -1 for individual player colors.
    public Color GetPlayerColor(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerColors.Length)
        {
            return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        return playerColors[playerIndex];
    }
}
