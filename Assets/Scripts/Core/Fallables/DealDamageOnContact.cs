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

    // Attached to both server and client versions of apple prefab
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "Platform" && other.gameObject.name != "Collector")
        {
            Debug.Log($"Trigger enter: {gameObject.name} collided with {other.gameObject.name}");
        }

        if (tag.Equals("FallingWord_Greyed")) return;

        //if (other.gameObject.name != "TopCollectorTrigger") return;

        if (other.attachedRigidbody == null) return;
        
        if (other.attachedRigidbody.TryGetComponent<BasketPlayer>(out BasketPlayer player))
        {
            //Debug.Log($"FallingWord has touched player {player.PlayerName.Value}");
            bool isWordCorrect = spawner.CheckIfCollectedWordIsCorrect(word);

            if (IsServer)
            {
                // Set up clientRpcParams to send to all clients
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = NetworkManager.ConnectedClientsIds
                    }
                };

                if (player.TryGetComponent<BasketHealth>(out BasketHealth health) &&
                    player.TryGetComponent<CoinWallet>(out CoinWallet wallet))
                {
                    if (!isWordCorrect)
                    {
                        ShowParticleEffectClientRpc(false, transform.position, clientRpcParams);
                        health.TakeDamage(damage);
                        wallet.CollectWord(word, isWordCorrect);
                    }
                    else
                    {
                        ShowParticleEffectClientRpc(true, transform.position, clientRpcParams);
                        wallet.CollectWord(word, isWordCorrect);
                    }
                }

                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    [ClientRpc]
    private void ShowParticleEffectClientRpc(bool isCorrect, Vector3 position, ClientRpcParams clientRpcParams)
    {
        GameObject particleEffect;
        if (isCorrect) particleEffect = rightCollectionParticleEffect;
        else particleEffect = wrongCollectionParticleEffect;

        Instantiate(particleEffect, position, Quaternion.identity);
    }
}
