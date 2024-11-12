using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public int MaxHealth = 100;
    public NetworkVariable<int> CurrentHealth;

	public ulong LastHitClientId => _lastHitClientId.Value;
	private NetworkVariable<ulong> _lastHitClientId = new NetworkVariable<ulong>();

	public Vector2 HitDirection => _hitDirection.Value;
	private NetworkVariable<Vector2> _hitDirection = new NetworkVariable<Vector2>();

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


		if (IsOwner)
		{
			UIManager.Get<UIViewManager>().GetView<GamePlayView>().InitHealthData(MaxHealth);
		}

		if (!IsHost) return;
		_lastHitClientId.Value = 256;
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

		if (IsOwner)
		{
			UIManager.Get<UIViewManager>().GetView<GamePlayView>().SetHealth(MaxHealth, newValue);
		}
	}

	public void Damage(int damageValue, ulong dealerId, Vector2 hitPoint = default(Vector2))
	{
		Debug.Log($"{damageValue} Damaged by {GameManager.Instance.NetworkServer.GetUserDataByClientID(dealerId).UserName}");

		SetDealer(dealerId);
		ModifyHealth(-damageValue);
		_hitDirection.Value = hitPoint;
	}

	public void SetDealer(ulong dealerId)
	{
		_lastHitClientId.Value = dealerId;
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
			OnDieEventClientRpc();
		}
	}

	[ClientRpc]
	private void OnDieEventClientRpc()
	{
		OnDie?.Invoke(this, EventArgs.Empty);
	}
}
