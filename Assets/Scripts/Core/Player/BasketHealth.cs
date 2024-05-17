using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BasketHealth : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealth { get; private set; } = 5;
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    public bool IsDead { get; private set; }

    public Action<BasketHealth> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (IsDead) return;

        int newHealth = CurrentHealth.Value + value;
        //Debug.Log("Before changing CurrentHealth");
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth);
        //Debug.Log("After changing CurrentHealth");

        if (CurrentHealth.Value == 0)
        {
            OnDie?.Invoke(this);
            IsDead = true;
        }
    }

}
