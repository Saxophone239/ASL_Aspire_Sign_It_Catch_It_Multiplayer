using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage color of each basket
/// </summary>
public class SICIPlayerColorDisplay : MonoBehaviour
{
    [SerializeField] private SICIColorLookup colorLookup;
    [SerializeField] private BasketPlayer player;
    [SerializeField] private SpriteRenderer[] playerSprites;
    private void Start()
    {
        Color playerColor = colorLookup.GetPlayerColor(-1);
        foreach (SpriteRenderer sprite in playerSprites)
        {
            sprite.color = playerColor;
        }
    }
}
