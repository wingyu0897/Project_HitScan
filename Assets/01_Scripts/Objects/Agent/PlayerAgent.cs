using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAgent : NetworkBehaviour
{
	public static EventHandler<PlayerEventArgs> OnPlayerDie;
	public static EventHandler<PlayerEventArgs> OnPlayerDespawn;
	public class PlayerEventArgs : EventArgs {
		public PlayerAgent Player;
	}

	[SerializeField] private SpriteRenderer _spriteRenderer;
	private Health _health;
	public Health Health => _health;

	private void Awake()
	{
		_health = GetComponent<Health>();
		_health.OnDie += HandleOnDie;
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		SetColorServerRpc();

		if (IsOwner)
		{
			UIViewManager.Instance.HideView<GameReadyView>();
			UIViewManager.Instance.ShowView<GamePlayView>();

			CameraManager.Instance.SetCameraTarget(transform);
			CameraManager.Instance.SetCamera(CAMERA_TYPE.Player);
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		OnPlayerDespawn?.Invoke(this, new PlayerEventArgs { Player = this });
		if (IsOwner)
		{
			UIViewManager.Instance.HideView<GamePlayView>();
			CameraManager.Instance.SetCamera(CAMERA_TYPE.Map);
			CameraManager.Instance.AimCamera(Vector2.zero, 1.0f, 0.0f);
		}
	}

	private void HandleOnDie(object sender, EventArgs e)
	{
		Kill();
	}

	public void Kill()
	{
		Destroy(gameObject);

		OnPlayerDie?.Invoke(this, new PlayerEventArgs { Player = this });
	}

	//[ClientRpc]
	//public void SetLayerClientRpc(int layer)
	//{
	//	gameObject.layer = layer;
	//	transform.Find("Visual").gameObject.layer = layer;
	//}

	//public void SetLayer(int layer)
	//{
	//	gameObject.layer = layer;
	//	transform.Find("Visual").gameObject.layer = layer;
	//}

	[ServerRpc(RequireOwnership = false)]
	private void SetColorServerRpc()
	{
		if (_spriteRenderer.color == Color.white)
			ApplyColorClientRpc(UnityEngine.Random.ColorHSV(0, 1f, 0, 1f, 0, 1f));
		else
			ApplyColorClientRpc(_spriteRenderer.color);
	}

	[ClientRpc]
	private void ApplyColorClientRpc(Color color)
	{
		_spriteRenderer.color = color;
	}
}
