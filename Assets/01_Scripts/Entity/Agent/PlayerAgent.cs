using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerAgent : NetworkBehaviour
{
	// Events
	public class PlayerEventArgs : EventArgs {
		public PlayerAgent Player;
		public ulong ClientID; }
	public static EventHandler<PlayerEventArgs> OnPlayerDie;
	public static EventHandler<PlayerEventArgs> OnPlayerSpawn;
	public static EventHandler<PlayerEventArgs> OnPlayerDespawn;

	// User Data
	public NetworkVariable<FixedString32Bytes> UserName;

	// References
	[SerializeField] private SpriteRenderer _spriteRenderer;
	[SerializeField] private WeaponHolder _weaponHolder;
	public WeaponHolder WeaponHolder => _weaponHolder;
	private Health _health;
	public Health Health => _health;

	// Prefabs
	[SerializeField] private ParticleSystem _deathEffect;

	// Flags
	private bool _isDead = false;

	private void Awake()
	{
		_health = GetComponent<Health>();
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		OnPlayerSpawn?.Invoke(this, new PlayerEventArgs { Player = this, ClientID = OwnerClientId });

		SetColorServerRpc();

		if (IsServer)
		{
			_health.OnDie += HandleOnDie;
			_isDead = false;
		}

		if (IsOwner)
		{
			UIManager.Get<UIViewManager>().HideView<GameReadyView>();
			UIManager.Get<UIViewManager>().ShowView<GamePlayView>();

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
			UIManager.Get<UIViewManager>().HideView<GamePlayView>();
		}
	}

	private void HandleOnDie(object sender, EventArgs e)
	{
		Kill();
	}

	public void Kill()
	{
		if (_isDead) return;

		_isDead = true;

		string killerName = GameManager.Instance.NetworkServer.GetUserDataByClientID(Health.LastHitClientId).UserName;
		KillClientRpc(killerName);
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
	private void KillClientRpc(string killerName, bool immediately = false)
	{
		Destroy(gameObject);

		OnPlayerDie?.Invoke(this, new PlayerEventArgs { Player = this, ClientID = OwnerClientId });
		UIManager.Get<UIViewManager>()?.GetView<DeathView>().SetKillerText(killerName);

		if (IsOwner)
		{
			if (immediately) // 데스캠 없이 즉시 메뉴로 되돌아 가기
			{
				CameraManager.Instance.AimCamera(Vector2.zero, 1.0f, 0.0f);
				UIManager.Get<UIViewManager>().ShowView<GameReadyView>();
				CameraManager.Instance.SetCameraType(CAMERA_TYPE.Map);
			}
			else // 데스캠을 보여준 후 메뉴로 되돌아 가기
			{
				UIManager.Get<UIViewManager>().ShowView<DeathView>();

				CameraManager.Instance.AimCamera(Vector2.zero, 1.0f, 0.0f);
				CameraManager.Instance.DeathCam(Health.LastHitClientId, 2.0f, () => {
					UIManager.Get<UIViewManager>().HideView<DeathView>();
					UIManager.Get<UIViewManager>().ShowView<GameReadyView>();
					CameraManager.Instance.SetCameraType(CAMERA_TYPE.Map);
				});
			}
		}

		DeathEffect();
	}

	/// <summary>
	/// Kill과 달리 데스캠 없이 준비 화면으로 돌아간다.
	/// </summary>
	public void KillImmediately()
	{
		if (_isDead) return;

		_isDead = true;

		string killerName = GameManager.Instance.NetworkServer.GetUserDataByClientID(Health.LastHitClientId).UserName;
		KillClientRpc(killerName, true);
	}

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

	private void DeathEffect()
	{
		Vector2 hitDirection = (Vector2)transform.position - Health.HitDirection;
		float hitAngle = Mathf.Atan2(hitDirection.y, hitDirection.x) * Mathf.Rad2Deg;
		ParticleSystem deathEffect = Instantiate(_deathEffect);
		var main = deathEffect.main;
		main.startColor = _spriteRenderer.color;
		deathEffect.transform.position = transform.position;
		deathEffect.transform.rotation = Quaternion.Euler(0, 0, hitAngle - 90);
		deathEffect.Play();
	}
}
