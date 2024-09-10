using Unity.Netcode;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
	[SerializeField] private Transform _weaponTrm;
	[Range(1f, 100f)]
	[SerializeField] private float _weaponLerp = 15f;

	[SerializeField] private Weapon _weapon;

    private Vector2 _beforePos;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (!IsOwner) return;

		_beforePos = transform.position;
	}

	private void FixedUpdate()
	{
		if (!IsOwner) return;

		SetRotation();
		SetPosition();
	}

	private void SetPosition()
	{
		_beforePos = Vector2.Lerp(_beforePos, transform.position, _weaponLerp * Time.deltaTime);
		_weaponTrm.position = _beforePos;
	}

	private void SetRotation()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 mouseDir = mousePos - _beforePos;
		float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

		_weaponTrm.localScale = new Vector3(1, (angle > 90f || angle < -90f) ? -1 : 1, 1);
		_weaponTrm.rotation = Quaternion.Euler(0, 0, angle);
	}

	public void TriggerOn()
	{
		TriggerOnServerRpc();
	}

	public void Attack()
	{
		AttackServerRpc();
	}

	public void TriggerOff()
	{
		TriggerOffServerRpc();
	}

	public void Reload()
	{
		ReloadServerRpc();
	}

	[ServerRpc]
	private void TriggerOnServerRpc()
	{
		_weapon.ClientId = OwnerClientId;
		_weapon?.TriggerOn();
	}
	[ServerRpc]
	private void AttackServerRpc()
	{
		_weapon?.Attack();
	}
	[ServerRpc]
	private void TriggerOffServerRpc()
	{
		_weapon?.TriggerOff();
	}
	[ServerRpc]
	private void ReloadServerRpc()
	{
		_weapon.Reload();
	}
}
