using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class FallingWord : Fallable
{   
    [Header("Prefabs")]
    [SerializeField] private GameObject wrongCollectionParticleEffect;
    [SerializeField] private GameObject rightCollectionParticleEffect;

    public event Action<FallingWord> OnCollected;
    public event Action<FallingWord> OnIncorrectCollected;

    public NetworkVariable<FixedString32Bytes> wordText = new NetworkVariable<FixedString32Bytes>();
    public int CoinValue = 10;

    private TextMeshProUGUI textDisplay;
    private Rigidbody2D rb;

    public override void OnNetworkSpawn()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        rb = GetComponent<Rigidbody2D>();

        //HandleWordNameChanged(string.Empty, (FixedString32Bytes) "(failed)");
        wordText.OnValueChanged += HandleWordNameChanged;
    }

    public override int Collect()
    {
        // Show particle effect
        Instantiate(rightCollectionParticleEffect, transform.position, Quaternion.identity);

        coinValue = CoinValue;

        GreyOutWords();

        if (!IsServer)
        {
            Show(false);
            Debug.Log("collecting coin but we're not the server");
            return 0;
        }

        if (alreadyCollected) return 0;

        alreadyCollected = true;
        Debug.Log($"correct word is collected: {wordText.Value}");

        OnCollected?.Invoke(this);
        return coinValue;
    }

    public int CollectIncorrect()
    {
        // Show particle effect
        Instantiate(wrongCollectionParticleEffect, transform.position, Quaternion.identity);

        if (!IsServer)
        {
            Show(false);
            return 0;
        }

        if (alreadyCollected) return 0;

        alreadyCollected = true;
        Debug.Log($"incorrect word is collected: {wordText.Value}");

        OnIncorrectCollected?.Invoke(this);
        return 0;
    }

    public void HideObject()
    {
        Show(false);
    }

    private void HandleWordNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        textDisplay.text = newName.ToString();
    }

    public void SetText(FixedString32Bytes newText)
    {
        wordText.Value = newText;
    }

    public void SetGravityScale(float newValue)
    {
        Rigidbody2D wordRigidBody = this.GetComponent<Rigidbody2D>();
        wordRigidBody.gravityScale = newValue;
        //rb.gravityScale = newValue;
    }

    private void GreyOutWords()
    {
        GameObject[] currentWords = GameObject.FindGameObjectsWithTag("FallingWord");

        foreach (GameObject word in currentWords)
        {
            if (word.TryGetComponent<FallingWord>(out FallingWord fallingWord))
            {
                fallingWord.GreyOut();
                fallingWord.tag = "FallingWord_Greyed";
            }
        }
    }
}
