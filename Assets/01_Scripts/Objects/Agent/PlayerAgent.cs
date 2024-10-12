using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAgent : NetworkBehaviour
{
	public class PlayerEventArgs : EventArgs {
		public PlayerAgent Player;
		public ulong ClientID; }
	public static EventHandler<PlayerEventArgs> OnPlayerDie;
	public static EventHandler<PlayerEventArgs> OnPlayerSpawn;
	public static EventHandler<PlayerEventArgs> OnPlayerDespawn;


	public string UserName;

	[SerializeField] private SpriteRenderer _spriteRenderer;
	private Health _health;
	public Health Health => _health;


	private void Awake()
	{
		_health = GetComponent<Health>();
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		Debug.Log("OnPlayerSpawn 실행됨");
		OnPlayerSpawn?.Invoke(this, new PlayerEventArgs { Player = this, ClientID = OwnerClientId });

		SetColorServerRpc();

		if (IsServer)
			_health.OnDie += HandleOnDie;

		if (IsOwner)
		{
			UIViewManager.Instance.HideView<GameReadyView>();
			UIViewManager.Instance.ShowView<GamePlayView>();

			CameraManager.Instance.SetCameraTarget(transform);
			CameraManager.Instance.SetCameraType(CAMERA_TYPE.Player);
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		OnPlayerDespawn?.Invoke(this, new PlayerEventArgs { Player = this, ClientID = OwnerClientId });
		if (IsOwner)
		{
			UIViewManager.Instance.HideView<GamePlayView>();
		}
	}

	private void HandleOnDie(object sender, EventArgs e)
	{
		Kill();
	}

	public void Kill()
	{
		KillClientRpc();
		//Destroy(gameObject);

		//OnPlayerDie?.Invoke(this, new PlayerEventArgs { Player = this, ClientID = OwnerClientId });
 
		//if (IsOwner)
		//{
		//	// 데스캠을 보여준 후 메뉴로 되돌아 가기
		//	CameraManager.Instance.AimCamera(Vector2.zero, 1.0f, 0.0f);
		//	CameraManager.Instance.DeathCam(Health.LastHitClientId, 2.0f, () => {
		//		UIViewManager.Instance.ShowView<GameReadyView>();
		//		CameraManager.Instance.SetCameraType(CAMERA_TYPE.Map);
		//	});
		//}	
	}

	[ClientRpc(RequireOwnership = false)]
	private void KillClientRpc(bool immediately = false)
	{
		Destroy(gameObject);

		OnPlayerDie?.Invoke(this, new PlayerEventArgs { Player = this, ClientID = OwnerClientId });

		if (IsOwner)
		{
			if (immediately)
			{
				CameraManager.Instance.AimCamera(Vector2.zero, 1.0f, 0.0f);
				UIViewManager.Instance.ShowView<GameReadyView>();
				CameraManager.Instance.SetCameraType(CAMERA_TYPE.Map);
			}
			else
			{
				// 데스캠을 보여준 후 메뉴로 되돌아 가기
				CameraManager.Instance.AimCamera(Vector2.zero, 1.0f, 0.0f);
				CameraManager.Instance.DeathCam(Health.LastHitClientId, 2.0f, () => {
					UIViewManager.Instance.ShowView<GameReadyView>();
					CameraManager.Instance.SetCameraType(CAMERA_TYPE.Map);
				});
			}
		}
	}

	/// <summary>
	/// Kill과 달리 데스캠 없이 준비 화면으로 돌아간다.
	/// </summary>
	public void KillImmediately()
	{
		//Destroy(gameObject);

		//OnPlayerDie?.Invoke(this, new PlayerEventArgs { Player = this, ClientID = OwnerClientId });

		//if (IsOwner)
		//{
		//	CameraManager.Instance.AimCamera(Vector2.zero, 1.0f, 0.0f);
		//	UIViewManager.Instance.ShowView<GameReadyView>();
		//	CameraManager.Instance.SetCameraType(CAMERA_TYPE.Map);
		//}

		KillClientRpc(true);
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
