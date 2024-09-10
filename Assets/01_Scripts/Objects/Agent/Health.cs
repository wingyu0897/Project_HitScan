using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public int MaxHealth = 100;
    public NetworkVariable<int> CurrentHealth;

	public ulong LastHitClientId { get; private set; }

	public event EventHandler OnDie;
	public event EventHandler<HealthChangeEventArgs> OnHealthChanged;
	public class HealthChangeEventArgs : EventArgs {
		public int Previous, New;
		public float Percentage;
	}

	public override void OnNetworkSpawn()
	{
		if (IsClient)
		{
			CurrentHealth.OnValueChanged += HandleHealthChanged;
			HandleHealthChanged(0, MaxHealth);
		}

		if (!IsHost) return;
		CurrentHealth.Value = MaxHealth;
	}

	public override void OnNetworkDespawn()
	{
		if (IsClient)
		{
			CurrentHealth.OnValueChanged -= HandleHealthChanged;
		}
	}

	private void HandleHealthChanged(int previousValue, int newValue)
	{
		OnHealthChanged?.Invoke(this, new HealthChangeEventArgs{ 
				Previous = previousValue,
				New = newValue,
				Percentage = (float)newValue / MaxHealth
			});
	}

	public void Damage(int damageValue, ulong dealerId)
	{
		Debug.Log($"{damageValue} Damaged by {GameManager.Instance.NetworkServer.GetUserDataByClientID(dealerId).UserName}");

		LastHitClientId = dealerId;
		ModifyHealth(-damageValue);
	}

	public void RestoreHealth(int healValue)
	{
		ModifyHealth(healValue);
	}

	public void ModifyHealth(int value)
	{
		if (!IsHost) return;

		if (CurrentHealth.Value == 0) return;
		CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value + value, 0, MaxHealth);
		if (CurrentHealth.Value == 0)
		{
			OnDie?.Invoke(this, EventArgs.Empty);
		}
	}
}
