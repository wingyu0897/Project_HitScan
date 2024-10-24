using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
	[SerializeField] private InputReaderPlayer _inputReader;

    private Movement _movement;
	private WeaponHolder _weaponHolder;

	private void Awake()
	{
		_movement = GetComponent<Movement>();
		_weaponHolder = GetComponent<WeaponHolder>();

		BindingInputs();
	}

	public override void OnNetworkSpawn()
	{
		BindingInputs();
	}

	public override void OnNetworkDespawn()
	{
		UnbindingInputs();
	}

	private void BindingInputs()
	{
		if (!IsOwner) return;

		_inputReader.OnMoveInput += MoveHandler;
		_inputReader.OnJumpBeginInput += JumpBeginHandler;
		_inputReader.OnJumpEndInput += JumpEndHandler;
		_inputReader.OnAttackBeginInput += AttackBeginHandler;
		_inputReader.OnAttackEndInput += AttackEndHandler;
		//_inputReader.OnAttackInput += AttackHandler;
		_inputReader.OnAimingBeginInput += AimingBeginHandler;
		_inputReader.OnAimingEndInput += AimingEndHandler;
		_inputReader.OnReloadInput += ReloadHandler;
	}

	private void UnbindingInputs()
	{
		if (!IsOwner) return;

		_inputReader.OnMoveInput -= MoveHandler;
		_inputReader.OnJumpBeginInput -= JumpBeginHandler;
		_inputReader.OnJumpEndInput -= JumpEndHandler;
		_inputReader.OnAttackBeginInput -= AttackBeginHandler;
		_inputReader.OnAttackEndInput -= AttackEndHandler;
		//_inputReader.OnAttackInput -= AttackHandler;
		_inputReader.OnAimingBeginInput -= AimingBeginHandler;
		_inputReader.OnAimingEndInput -= AimingEndHandler;
		_inputReader.OnReloadInput -= ReloadHandler;
	}

	private void MoveHandler(Vector2 direction) => _movement?.SetMove(direction);

	private void JumpBeginHandler() => _movement?.SetJump(true);
	private void JumpEndHandler() => _movement?.SetJump(false);

	private void AttackBeginHandler() => _weaponHolder?.TriggerOn();
	private void AttackEndHandler() => _weaponHolder?.TriggerOff();
	//private void AttackHandler() => _weaponHolder?.TryAttack();

	private void AimingBeginHandler() => _weaponHolder?.Aiming();
	private void AimingEndHandler() => _weaponHolder?.CompleteAiming();

	private void ReloadHandler() => _weaponHolder.Reload();
}
