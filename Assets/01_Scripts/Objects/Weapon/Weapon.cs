using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
	private SpriteRenderer _spriteRen;
	private WeaponRecoil _weaponRecoil;
	private WeaponAnimator _weaponAnimator;
	private AudioPlayer _audioPlayer;

    [SerializeField] private WeaponDataSO _data;
	public WeaponDataSO Data => _data;
	private List<TrailRenderer> _trails = new List<TrailRenderer>();

	private NetworkVariable<int> _ammo = new NetworkVariable<int>();
	public int Ammo => _ammo.Value;
	private bool _isTriggered;
	private bool _isReloading = false;
	private float _lastFireTime;

    [HideInInspector] public ulong ClientId;

	public event Action OnReloaded;

	private void Awake()
	{
		_spriteRen = GetComponent<SpriteRenderer>();
		_weaponRecoil = GetComponent<WeaponRecoil>();
		_weaponAnimator = GetComponent<WeaponAnimator>();
		_audioPlayer = GetComponent<AudioPlayer>();	
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (_trails.Count > 0)
		{
			StopAllCoroutines();
			_trails.ForEach(t => Destroy(t.gameObject));
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (IsHost)
		{
			ChangeWeapon(_data);
		}
	}

	public void ChangeWeapon(WeaponDataSO data)
	{
		if (_isReloading) StopAllCoroutines();

		_data = data;
		_weaponAnimator.ChangeVisual(data.LibraryAsset);
		//_spriteRen.sprite = _data.Visual;
		_ammo.Value = _data.MaxAmmo;
		_lastFireTime = -_data.FireRate;
	}

	public void ChangeWeaponClient(WeaponDataSO data)
	{
		_data = data;
		_weaponAnimator.ChangeVisual(data.LibraryAsset);
		//_spriteRen.sprite = _data.Visual;
	}

	#region Attack
	public virtual void TriggerOn()
	{
		TriggerOnServerRpc();
	}

	// 나중에 서버에서 계산하는 것으로 다 바꿔야 할 듯?
	[ServerRpc]
	private void TriggerOnServerRpc()
	{
		if (_ammo.Value == 0)
		{
			// 총알이 없으면 재장전한다
			Reload();
		}
		else if (!_data.IsAuto)
		{
			// 단발 사격일 경우 바로 사격한다
			TryAttack();
		}

		_isTriggered = true;
	}

	public virtual void TryAttack()
	{
		TryAttackServerRpc();
	}

	// *주의* WeaponScript의 모든 ServerRpc는 RequireOwnership = false를 해주어야 오너가 아닌 서버에서도 실행할 수 있다. 아니면 에러 발생
	/// <summary>
	/// 공격이 가능한지 판단하는 서버Rpc 함수
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	private void TryAttackServerRpc()
	{
		if (_isReloading || _ammo.Value == 0 || (_isTriggered && !_data.IsAuto)) // 재장전 중 or 총알 없음 or 연사가 아니면서 이미 발사됨
			return;
		if (Time.time - _lastFireTime < _data.FireRate) // 발사 쿨타임 중일 경우
			return;

		AttackClientRpc();
		AttackServerRpc();
	}

	[ClientRpc]
	private void AttackClientRpc()
	{
		_audioPlayer.PlayAudio(_data.FireClip);

		if (IsOwner)
		{
			_weaponAnimator.Fire();
			DrawTraceImmediatley();
			_weaponRecoil.DoRecoil(_data.RecoilAngle, _data.RecoilDistance);
		}
	}

	/// <summary>
	///  실제로 공격 여부를 판정하는 서버 RPC 공격 함수
	/// </summary>
	/// <param name="clientId">공격하는 클라이언트</param>
	[ServerRpc(RequireOwnership = false)]
	private void AttackServerRpc()
	{
		//ClientId = clientId;

		_lastFireTime = Time.time;
		--_ammo.Value;

		// --- 레이캐스트 ---
		int layer = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Player"));

		RaycastHit2D[] hits = new RaycastHit2D[2];
		Physics2D.RaycastNonAlloc(transform.position, transform.right, hits, _data.FireRange, layer);

		for (int i = 0; i < hits.Length; ++i)
		{
			if (hits[i].collider != null)
			{
				if (hits[i].collider.attachedRigidbody != null && hits[i].collider.attachedRigidbody.TryGetComponent(out Health health))
				{
					if (health.OwnerClientId == OwnerClientId)
					{
						continue;
					}

					if (GameManager.Instance.IsAttackable(OwnerClientId, health.OwnerClientId)) // 아군이 아닌지 확인
					{
						hits[i].collider.attachedRigidbody?.AddForce(transform.right, ForceMode2D.Impulse);
						health.Damage(_data.Damage, OwnerClientId, hits[i].point);
					}
				}

				float tracerTime = 0.1f * (Vector2.Distance(transform.position, hits[i].point) / _data.FireRange);
				DrawTraceClientRpc(transform.position, hits[i].point, tracerTime);
				return;
			}
		}

		// 맞은 표적이 없을 때
		DrawTraceClientRpc(transform.position, transform.position + transform.right * _data.FireRange, 0.1f);
	}

	public virtual void TriggerOff()
	{
		TriggerOffServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void TriggerOffServerRpc()
	{
		_isTriggered = false;
	}

	public void Reload()
	{
		ReloadServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void ReloadServerRpc()
	{
		if (_isReloading) return;

		_isReloading = true;
		StartReloadClientRpc();
		StartCoroutine(ReloadCo());
	}

	IEnumerator ReloadCo()
	{
		float timer = _data.ReloadTime;

		_weaponAnimator.SetSpeed(1f / timer);
		_weaponAnimator.Reload();

		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}

		_ammo.Value = _data.MaxAmmo;
		_isReloading = false;

		_weaponAnimator.SetSpeed(1f);
		ReloadedClientRpc();
	}

	[ClientRpc]
	private void StartReloadClientRpc()
	{
		if (IsOwner)
		{
			_audioPlayer.PlayAudio(_data.ReloadClip); // 재장전 사운드는 자신의 클라이언트에서만 재생된다
		}
	}

	[ClientRpc(RequireOwnership = false)]
	private void ReloadedClientRpc()
	{
		OnReloaded?.Invoke();
	}

	[ClientRpc]
	private void EmptyClipClientRpc()
	{
		if (IsOwner)
			_audioPlayer.PlayAudio(_data.EmptyClip);
	}
	#endregion

	#region Draw Trace
	public void DrawTraceImmediatley()
	{
		int layer = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Player"));

		RaycastHit2D[] hits = new RaycastHit2D[2];
		Physics2D.RaycastNonAlloc(transform.position, transform.right, hits, _data.FireRange, layer);

		for (int i = 0; i < hits.Length; ++i)
		{
			if (hits[i].collider != null)
			{
				if (hits[i].collider.attachedRigidbody != null && hits[i].collider.attachedRigidbody.TryGetComponent(out Health health))
				{
					if (health.OwnerClientId == ClientId)
					{
						continue;
					}
				}

				float tracerTime = 0.1f * (Vector2.Distance(transform.position, hits[i].point) / _data.FireRange);
				DrawTrace(transform.position, hits[i].point, tracerTime);
				return;
			}
		}

		DrawTrace(transform.position, transform.position + transform.right * _data.FireRange, 0.1f);
	}

	[ClientRpc]
	private void DrawTraceClientRpc(Vector2 start, Vector2 end, float time)
	{
		if (IsOwner) return;

		DrawTrace(start, end, time);
	}

	private void DrawTrace(Vector2 startPoint, Vector2 hitPoint, float time)
	{
		TrailRenderer trail = Instantiate(_data._bulletTrailPrefab, transform.position, Quaternion.identity);

		StartCoroutine(DrawTraceCo(trail, startPoint, hitPoint, time));
	}

	IEnumerator DrawTraceCo(TrailRenderer trail, Vector2 startPoint, Vector2 hitPoint, float time)
	{
		_trails.Add(trail);
		trail.Clear();
		float timer = time;
		while (timer > 0)
		{
			timer -= Time.deltaTime;
			trail.transform.position = Vector2.Lerp(hitPoint, startPoint, timer / time);
			yield return null;
		}

		trail.transform.position = hitPoint;

		yield return new WaitForSeconds(trail.time);

		Destroy(trail.gameObject);
		_trails.Remove(trail);
	}
	#endregion
}
