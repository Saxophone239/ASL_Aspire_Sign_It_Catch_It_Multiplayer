using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public abstract class Fallable : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private TextMeshProUGUI[] texts;

    protected int coinValue = 10;
    protected bool alreadyCollected = false;

    // Returns amount of points player gets when collected
    public abstract int Collect();

    public void SetValue(int value)
    {
        coinValue = value;
    }

    protected void Show(bool show)
    {
        Debug.Log($"Showing fallable with parameter: {show}");
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.enabled = show;
        }
        foreach (TextMeshProUGUI text in texts)
        {
            text.enabled = show;
        }
    }

    protected void GreyOut()
    {
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = new Color32(255, 255, 255, 36);
        }
        foreach (TextMeshProUGUI text in texts)
        {
            text.color = Color.gray;
        }
    }
}
