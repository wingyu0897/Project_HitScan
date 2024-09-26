using Unity.Netcode;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    private Movement _movement;
	private WeaponHolder _weaponHolder;

	private void Awake()
	{
		_movement = GetComponent<Movement>();
		_weaponHolder = GetComponent<WeaponHolder>();
	}

	private void Update()
	{
		MovementInput();
		WeaponInput();
	}

	private void MovementInput()
	{
		if (!IsOwner) return;

		if (Input.GetKey(KeyCode.D)) _movement.SetMove(Vector2.right);
		if (Input.GetKey(KeyCode.A)) _movement.SetMove(-Vector2.right);

		if (Input.GetKey(KeyCode.W))
			_movement.Jump();
	}

	private void WeaponInput()
	{
		if (!IsOwner) return;

		if (Input.GetMouseButtonDown(0))
			_weaponHolder.TriggerOn();
		if (Input.GetMouseButtonUp(0))
			_weaponHolder.TriggerOff();
		if (Input.GetMouseButton(0))
			_weaponHolder.Attack();

		if (Input.GetKeyDown(KeyCode.R))
			_weaponHolder.Reload();

		if (Input.GetMouseButton(1))
			_weaponHolder.Aiming();
		if (Input.GetMouseButtonUp(1))
			_weaponHolder.CompleteAiming();
	}
}
