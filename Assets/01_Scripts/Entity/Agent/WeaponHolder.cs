using Unity.Netcode;
using UnityEngine;

public class WeaponHolder : NetworkBehaviour
{
	private Movement _movement;

	[SerializeField] private WeaponsSO _weapons; // 플레이어가 선택할 수 있는 무기의 데이터
	[SerializeField] private Weapon _weapon; // 무기 참조

	[SerializeField] private Transform _weaponTrm; // 무기 트랜스폼
	[Range(1f, 100f)]
	[SerializeField] private float _weaponLerp = 15f; // 무기의 움직임 러프

	private bool _isTriggered = false;
	private bool _isAiming = false;
    private Vector2 _beforePos;
	private Vector3 _mouseWorldPos;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		SetWeaponVisualServerRpc();

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

	private void Update()
	{
		if (!IsOwner) return;

		if (_isTriggered)
			TryAttack();

		if (_isAiming)
		{
			// 뷰포트 좌표(0 ~ 1)에서 0.5를 낮추어 범위를 -0.5 ~ 0.5로 제한한다
			// 0.9를 곱하여 범위(-0.5 ~ 0.5) 중에서 -0.45부터 0.45 까지는 조준 거리가 비례한다
			Vector3 mouseViewPos = (Camera.main.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f)) / 0.9f;
			float distance = Mathf.Clamp(Mathf.Abs(mouseViewPos.magnitude) * 2.0f, 0, 1.0f) * _weapon.Data.AimingDistance;

			Vector3 mouseDir = _mouseWorldPos - transform.position;
			mouseDir.z = 0;
			mouseDir.Normalize();

			Vector3 targetPos = mouseDir * distance;

			CameraManager.Instance.AimCamera(targetPos, _weapon.Data.AimingOrthoRatio);
		}
	}

	/// <summary>
	/// 플레이어가 생성될 때 서버에서 실행하는 무기 변경 함수
	/// </summary>
	//[ServerRpc(RequireOwnership = false)]
	public void ChangeWeaponServer(string weaponId)
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

	/// <summary>
	/// 다른 플레이어가 접속했을 때, 이미 접속해 있는 플레이어의 무기 비주얼을 동기화하는 작업
	/// </summary>
	[ServerRpc(RequireOwnership = false)]
	private void SetWeaponVisualServerRpc()
	{
		if (_weapon.Data == null) return;

		ChangeWeaponClientRpc(_weapon.Data.Name);
	}

	public void SetWeaponUI(WeaponDataSO weapon)
	{
		UIManager.Get<UIViewManager>().GetView<GamePlayView>().InitWeaponData(weapon.MaxAmmo, weapon.Visual);
	}

	private void HandleReloaded()
	{
		UIManager.Get<UIViewManager>().GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
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
		_isTriggered = true;
		_weapon.ClientId = OwnerClientId;
		_weapon?.TriggerOn();
		UIManager.Get<UIViewManager>().GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	public void TryAttack()
	{
		_weapon?.TryAttack();
		UIManager.Get<UIViewManager>().GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
	}

	public void TriggerOff()
	{
		_isTriggered = false;

		_weapon?.TriggerOff();
		UIManager.Get<UIViewManager>().GetView<GamePlayView>().SetCurrentAmmo(_weapon.Ammo);
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

		_isAiming = true;
		_movement.SetSlowdown(_weapon.Data.AimingSlowdown);
	}

	public void CompleteAiming()
	{
		CameraManager.Instance.AimCamera(Vector2.zero);
		_isAiming = false;
		_movement.SetSlowdown(0f);
	}
}
