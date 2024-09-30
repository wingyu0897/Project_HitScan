using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private WeaponDataSO _data;
	public WeaponDataSO Data => _data;

	private int _ammo;
	public int Ammo => _ammo;
	private bool _isTriggered;
	private bool _isReloading = false;
	private float _lastFireTime;

    [HideInInspector] public ulong ClientId;

	public event Action OnReloaded;

	private void Awake()
	{
		_ammo = _data.MaxAmmo;
		_lastFireTime = -_data.FireRate;
	}

	#region Attack  // 공격 가능 여부 및 공격 가능 여부는 각 클라이언트에서 게산
	public virtual void TriggerOn()
	{
		if (_ammo == 0)
		{
			// 총알이 없으면 재장전한다
			Reload();
		}
		else if (!_data.IsAuto)
		{
			// 단발 사격일 경우 바로 사격한다
			Attack();
		}

		_isTriggered = true;
	}

	public virtual bool Attack()
	{
		if (_isReloading ||  _ammo == 0 || (_isTriggered && !_data.IsAuto)) return false; // 재장전 중 or 총알 없음 or 연사 불가능 상태일 경우
		if (Time.time - _lastFireTime < _data.FireRate) return false; // 쿨타임 중일 경우

		_lastFireTime = Time.time;

		DrawTraceImmediatley();		// 발사한 클라이언트에서는 즉시 총알의 궤적을 계산하여 그려준다. 다른 클라이언트는 서버에서 계산된 총알의 궤적을 통해 그려준다
		AttackServerRpc(ClientId);	// 서버 Rpc를 통해 서버에서 공격을 계산한다
		
		--_ammo;

		return true;
	}

	/// <summary>
	///  실제로 공격 여부를 판정하는 서버 RPC 공격 함수
	/// </summary>
	/// <param name="clientId">공격하는 클라이언트</param>
	[ServerRpc]
	private void AttackServerRpc(ulong clientId)
	{
		ClientId = clientId;

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

					if (GameManager.Instance.IsAttackable(ClientId, health.OwnerClientId)) // 아군이 아닌지 확인
					{
						health.Damage(_data.Damage, ClientId);
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
		_isTriggered = false;
	}

	public void Reload()
	{
		if (_isReloading) return; // 장전 중

		_isReloading = true;
		StartCoroutine(ReloadCo());
	}

	IEnumerator ReloadCo()
	{
		float timer = _data.ReloadTime;

		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}

		_ammo = _data.MaxAmmo;
		_isReloading = false;
		OnReloaded?.Invoke();
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
	}
	#endregion
}
