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
		if (!IsOwner) return;

		if (Input.GetKey(KeyCode.D)) _movement.SetMove(Vector2.right);
		if (Input.GetKey(KeyCode.A)) _movement.SetMove(-Vector2.right);

		if (Input.GetKey(KeyCode.W))
			_movement.Jump();

		if (Input.GetMouseButtonDown(0))
		{
			_weaponHolder.Attack();
		}
	}
}
