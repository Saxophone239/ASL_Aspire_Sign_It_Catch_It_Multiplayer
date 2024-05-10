using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject wrongCollectionParticleEffect;
    [SerializeField] private GameObject rightCollectionParticleEffect;
    [SerializeField] private FallingWord word;
    [SerializeField] private int damage = 1;

    private SICISpawner spawner;

    private void Awake()
    {
        spawner = FindObjectOfType<SICISpawner>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "Platform" && other.gameObject.name != "Collector")
        {
            Debug.Log($"Trigger enter: {gameObject.name} collided with {other.gameObject.name}");
        }

        if (tag.Equals("FallingWord_Greyed")) return;

        if (other.gameObject.name != "TopCollectorTrigger") return;

        if (other.attachedRigidbody == null) return;
        
        if (other.attachedRigidbody.TryGetComponent<BasketPlayer>(out BasketPlayer player))
        {
            //Debug.Log($"FallingWord has touched player {player.PlayerName.Value}");
            bool isWordCorrect = spawner.CheckIfCollectedWordIsCorrect(word);

            if (player.TryGetComponent<BasketHealth>(out BasketHealth health) &&
                player.TryGetComponent<CoinWallet>(out CoinWallet wallet))
            {
                if (!isWordCorrect)
                {
                    // Show particle effect
                    Instantiate(wrongCollectionParticleEffect, transform.position, Quaternion.identity);
                    if (IsServer) health.TakeDamage(damage);
                    wallet.CollectWord(word, isWordCorrect);
                }
                else
                {
                    Instantiate(rightCollectionParticleEffect, transform.position, Quaternion.identity);
                    wallet.CollectWord(word, isWordCorrect);
                }
            }
        }
    }
}
