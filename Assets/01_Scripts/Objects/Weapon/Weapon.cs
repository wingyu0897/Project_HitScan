using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    [SerializeField] private WeaponDataSO _data;

	private int _ammo;
	private bool _isTriggered;
	private bool _isReloading = false;
	private float _lastFireTime;

    public ulong ClientId;

	private void Awake()
	{
		_ammo = _data.MaxAmmo;
		_lastFireTime = -_data.FireRate;
	}

	public virtual void TriggerOn()
	{
		if (_ammo == 0)
		{
			Reload();
		}
		else if (!_data.IsAuto)
		{
			Attack();
		}

		_isTriggered = true;
	}

	public virtual void Attack()
	{
		if (_isReloading ||  _ammo == 0 || (_isTriggered && !_data.IsAuto)) return; // 재장전 중 or 총알 없음 or 연사 불가능
		if (Time.time - _lastFireTime < _data.FireRate) return; // 쿨타임

		_lastFireTime = Time.time;


		string oppent = GameManager.Instance.GetTeam(ClientId) == TEAM_TYPE.Red ? "Blue" : "Red";
		int layer = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer(oppent));
		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, _data.FireRange, layer);

		DrawTraceClientRpc(oppent);

		if (hit.collider != null)
		{
			if (hit.collider.attachedRigidbody != null && hit.collider.attachedRigidbody.TryGetComponent(out Health health))
			{
				if (GameManager.Instance.IsAttackable(ClientId, health.OwnerClientId)) // 아군이 아닌지 확인
				{
					health.Damage(_data.Damage, ClientId);
				}
			}

			//float tracerTime = Vector2.Distance(transform.position, hit.point) < 5f ? 0f : 0.1f;
			//DrawTraceClientRpc(transform.position, hit.point, tracerTime);
		}
		//else
			//DrawTraceClientRpc(transform.position, transform.right * _data.FireRange, 0.1f);

		--_ammo;

		//AttackServerRpc();
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
	}

	[ClientRpc]
	private void DrawTraceClientRpc(string oppent)
	{
		int layer = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer(oppent));
		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, _data.FireRange, layer);

		if (hit.collider != null)
		{
			float tracerTime = Vector2.Distance(transform.position, hit.point) < 5f ? 0f : 0.1f;
			DrawTrace(transform.position, hit.point, tracerTime);
		}
		else
			DrawTrace(transform.position, transform.right * _data.FireRange, 0.1f);
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
}
