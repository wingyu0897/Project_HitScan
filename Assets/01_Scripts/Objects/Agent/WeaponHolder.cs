using Unity.Netcode;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
	[SerializeField] private Transform _weaponTrm;
	[Range(1f, 100f)]
	[SerializeField] private float _weaponLerp = 15f;

	[SerializeField] private Weapon _weapon;

    private Vector2 _beforePos;
	private Vector3 _mouseWorldPos;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (!IsOwner) return;

		_beforePos = transform.position;
		SetWeapon(_weapon);
	}

	private void FixedUpdate()
	{
		if (!IsOwner) return;

		_mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		SetRotation();
		SetPosition();
	}

	public void SetWeapon(Weapon weapon)
	{
		_weapon = weapon;
		_weapon.OnReloaded += HandleReloaded;
		UIViewManager.Instance.GetView<GamePlayView>().InitWeaponData(_weapon.Data.MaxAmmo, _weapon.Data.Visual);
	}

	private void HandleReloaded()
	{
		UIViewManager.Instance.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	private void SetPosition()
	{
		_beforePos = Vector2.Lerp(_beforePos, transform.position, _weaponLerp * Time.deltaTime);
		_weaponTrm.position = _beforePos;
	}

	private void SetRotation()
	{
		Vector2 mousePos = _mouseWorldPos;
		Vector2 mouseDir = mousePos - _beforePos;
		float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

		_weaponTrm.localScale = new Vector3(1, (angle > 90f || angle < -90f) ? -1 : 1, 1);
		_weaponTrm.rotation = Quaternion.Euler(0, 0, angle);
	}

	public void TriggerOn()
	{
		_weapon.ClientId = OwnerClientId;
		_weapon?.TriggerOn();
		UIViewManager.Instance.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	public void Attack()
	{
		if (_weapon.Attack())
		{
			UIViewManager.Instance.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
		}
	}

	public void TriggerOff()
	{
		_weapon?.TriggerOff();
		UIViewManager.Instance.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	public void Reload()
	{
		_weapon.Reload();
	}

	/// <summary>
	/// ����. ī�޶� ������ �� ���� �� �� �ְ� �ȴ�.
	/// </summary>
	public void Aiming()
	{
		if (!IsOwner) return;

		// ����Ʈ ��ǥ(0 ~ 1)���� 0.5�� ���߾� ������ -0.5 ~ 0.5�� �����Ѵ�
		// 0.9�� ���Ͽ� ����(-0.5 ~ 0.5) �߿��� -0.45���� 0.45 ������ ���� �Ÿ��� ����Ѵ�
		Vector3 mousePos = (Camera.main.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f)) / 0.9f;
		float distance = Mathf.Clamp(Mathf.Abs(mousePos.magnitude) * 2.0f, 0, 1.0f) * _weapon.Data.AimingDistance;

		Vector3 mouseDir = _mouseWorldPos - transform.position;
		mouseDir.z = 0;
		mouseDir.Normalize();

		Vector3 targetPos = mouseDir * distance;
		
		CameraManager.Instance.AimCamera(targetPos, _weapon.Data.AimingOrthoRatio);
	}

	public void CompleteAiming()
	{
		CameraManager.Instance.AimCamera(Vector2.zero);
	}
}
