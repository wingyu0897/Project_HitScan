using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
	private SpriteRenderer _spriteRen;

    [SerializeField] private WeaponDataSO _data;
	public WeaponDataSO Data => _data;
	private List<TrailRenderer> _trails = new List<TrailRenderer>();

	private int _ammo;
	public int Ammo => _ammo;
	private bool _isTriggered;
	private bool _isReloading = false;
	private float _lastFireTime;

    [HideInInspector] public ulong ClientId;

	public event Action OnReloaded;

	private void Awake()
	{
		_spriteRen = GetComponent<SpriteRenderer>();
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
		_data = data;
		_spriteRen.sprite = _data.Visual;
		_ammo = _data.MaxAmmo;
		_lastFireTime = -_data.FireRate;
	}

	public void ChangeWeaponClient(WeaponDataSO data)
	{
		_data = data;
		_spriteRen.sprite = _data.Visual;
	}

	#region Attack
	public virtual void TriggerOn()
	{
		TriggerOnServerRpc();
	}

	// ���߿� �������� ����ϴ� ������ �� �ٲ�� �� ��?
	[ServerRpc]
	private void TriggerOnServerRpc()
	{
		if (_ammo == 0)
		{
			// �Ѿ��� ������ �������Ѵ�
			Reload();
		}
		else if (!_data.IsAuto)
		{
			// �ܹ� ����� ��� �ٷ� ����Ѵ�
			TryAttack();
		}

		_isTriggered = true;
	}

	public virtual void TryAttack()
	{
		TryAttackServerRpc(ClientId);
	}

	/// <summary>
	/// ������ �������� �Ǵ��ϴ� ����Rpc �Լ�
	/// </summary>
	[ServerRpc]
	private void TryAttackServerRpc(ulong clientId)
	{
		if (_isReloading || _ammo == 0 || (_isTriggered && !_data.IsAuto)) return; // ������ �� or �Ѿ� ���� or ���簡 �ƴ�
		if (Time.time - _lastFireTime < _data.FireRate) return; // �߻� ��Ÿ�� ���� ���

		AttackClientRpc();
		AttackServerRpc(clientId);
	}

	[ClientRpc]
	private void AttackClientRpc()
	{
		if (IsOwner)
			DrawTraceImmediatley();
	}

	/// <summary>
	///  ������ ���� ���θ� �����ϴ� ���� RPC ���� �Լ�
	/// </summary>
	/// <param name="clientId">�����ϴ� Ŭ���̾�Ʈ</param>
	[ServerRpc]
	private void AttackServerRpc(ulong clientId)
	{
		ClientId = clientId;

		_lastFireTime = Time.time;
		--_ammo;

		// --- ����ĳ��Ʈ ---
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

					if (GameManager.Instance.IsAttackable(ClientId, health.OwnerClientId)) // �Ʊ��� �ƴ��� Ȯ��
					{
						health.Damage(_data.Damage, ClientId, hits[i].point);
					}
				}

				float tracerTime = 0.1f * (Vector2.Distance(transform.position, hits[i].point) / _data.FireRange);
				DrawTraceClientRpc(transform.position, hits[i].point, tracerTime);
				return;
			}
		}

		// ���� ǥ���� ���� ��
		DrawTraceClientRpc(transform.position, transform.position + transform.right * _data.FireRange, 0.1f);
	}

	public virtual void TriggerOff()
	{
		TriggerOffServerRpc();
	}

	[ServerRpc]
	private void TriggerOffServerRpc()
	{
		_isTriggered = false;
	}

	public void Reload()
	{
		if (!IsServer) return;

		if (_isReloading) return; // ���� ��

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

		ReloadedClientRpc();
	}

	[ClientRpc]
	private void ReloadedClientRpc()
	{
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
		//if (IsOwner) return;

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
