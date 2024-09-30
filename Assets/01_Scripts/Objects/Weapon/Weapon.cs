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

	#region Attack  // ���� ���� ���� �� ���� ���� ���δ� �� Ŭ���̾�Ʈ���� �Ի�
	public virtual void TriggerOn()
	{
		if (_ammo == 0)
		{
			// �Ѿ��� ������ �������Ѵ�
			Reload();
		}
		else if (!_data.IsAuto)
		{
			// �ܹ� ����� ��� �ٷ� ����Ѵ�
			Attack();
		}

		_isTriggered = true;
	}

	public virtual bool Attack()
	{
		if (_isReloading ||  _ammo == 0 || (_isTriggered && !_data.IsAuto)) return false; // ������ �� or �Ѿ� ���� or ���� �Ұ��� ������ ���
		if (Time.time - _lastFireTime < _data.FireRate) return false; // ��Ÿ�� ���� ���

		_lastFireTime = Time.time;

		DrawTraceImmediatley();		// �߻��� Ŭ���̾�Ʈ������ ��� �Ѿ��� ������ ����Ͽ� �׷��ش�. �ٸ� Ŭ���̾�Ʈ�� �������� ���� �Ѿ��� ������ ���� �׷��ش�
		AttackServerRpc(ClientId);	// ���� Rpc�� ���� �������� ������ ����Ѵ�
		
		--_ammo;

		return true;
	}

	/// <summary>
	///  ������ ���� ���θ� �����ϴ� ���� RPC ���� �Լ�
	/// </summary>
	/// <param name="clientId">�����ϴ� Ŭ���̾�Ʈ</param>
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

					if (GameManager.Instance.IsAttackable(ClientId, health.OwnerClientId)) // �Ʊ��� �ƴ��� Ȯ��
					{
						health.Damage(_data.Damage, ClientId);
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
		_isTriggered = false;
	}

	public void Reload()
	{
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
