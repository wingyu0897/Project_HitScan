using Unity.Netcode;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
	private Movement _movement;

	[SerializeField] private WeaponsSO _weapons;

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

		_movement = GetComponent<Movement>();

		_beforePos = transform.position;
		_weapon.OnReloaded += HandleReloaded;

		SetWeaponUI(_weapon.Data);
	}

	private void FixedUpdate()
	{
		if (!IsOwner) return;

		_mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		SetRotation();
		SetPosition();
	}

	/// <summary>
	/// 플레이어가 생성될 때 서버에서 실행하는 무기 변경 함수
	/// </summary>
	[ServerRpc]
	public void ChangeWeaponServerRpc(string weaponId)
	{
		WeaponDataSO weapon = _weapons.Weapons.Find(x => x.Name == weaponId);
		if (weapon != null)
		{
			_weapon.ChangeWeapon(weapon);
			ChangeWeaponClientRpc(weaponId);
		}
	}

	[ClientRpc]
	public void ChangeWeaponClientRpc(string weaponId)
	{
		WeaponDataSO weapon = _weapons.Weapons.Find(x => x.Name == weaponId);
		if (weapon != null)
		{
			_weapon.ChangeWeaponClient(weapon);
			if (IsOwner)
			{
				SetWeaponUI(weapon);
			}
		}
	}

	public void SetWeaponUI(WeaponDataSO weapon)
	{
		UIManager.UIViewManager.GetView<GamePlayView>().InitWeaponData(weapon.MaxAmmo, weapon.Visual);
	}

	private void HandleReloaded()
	{
		UIManager.UIViewManager.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
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
		UIManager.UIViewManager.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	public void TryAttack()
	{
		_weapon.TryAttack();
		UIManager.UIViewManager.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	public void TriggerOff()
	{
		_weapon?.TriggerOff();
		UIManager.UIViewManager.GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	public void Reload()
	{
		_weapon.Reload();
	}

	/// <summary>
	/// 조준. 카메라를 움직여 먼 곳을 볼 수 있게 된다.
	/// </summary>
	public void Aiming()
	{
		if (!IsOwner) return;

		// 뷰포트 좌표(0 ~ 1)에서 0.5를 낮추어 범위를 -0.5 ~ 0.5로 제한한다
		// 0.9를 곱하여 범위(-0.5 ~ 0.5) 중에서 -0.45부터 0.45 까지는 조준 거리가 비례한다
		Vector3 mousePos = (Camera.main.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f)) / 0.9f;
		float distance = Mathf.Clamp(Mathf.Abs(mousePos.magnitude) * 2.0f, 0, 1.0f) * _weapon.Data.AimingDistance;

		Vector3 mouseDir = _mouseWorldPos - transform.position;
		mouseDir.z = 0;
		mouseDir.Normalize();

		Vector3 targetPos = mouseDir * distance;
		
		CameraManager.Instance.AimCamera(targetPos, _weapon.Data.AimingOrthoRatio);
		_movement.SetSlowdown(_weapon.Data.AimingSlowdown);
	}

	public void CompleteAiming()
	{
		CameraManager.Instance.AimCamera(Vector2.zero);
		_movement.SetSlowdown(0f);
	}
}
