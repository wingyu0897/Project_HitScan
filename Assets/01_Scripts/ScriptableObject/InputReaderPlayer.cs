using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "InputReaderPlayer")]
public class InputReaderPlayer : InputReader, InputSystem.IPlayerActions
{
	protected override void OnEnable()
	{
		base.OnEnable();

		input.Player.AddCallbacks(this);
	}

	public Action OnAttackBeginInput;
	public Action OnAttackInput;
	public Action OnAttackEndInput;
	public Action OnAimingBeginInput;
	public Action OnAimingEndInput;
	public Action OnReloadInput;
	public Action OnJumpBeginInput;
	public Action OnJumpEndInput;
	public Action<Vector2> OnMoveInput;

	public void OnAttack(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnAttackBeginInput?.Invoke();
			return;
		}
		else if (context.canceled)
		{
			OnAttackEndInput?.Invoke();
			return;
		}

		OnAttackInput?.Invoke();
	}

	public void OnAiming(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnAimingBeginInput?.Invoke();
		}
		else if (context.canceled)
		{
			OnAimingEndInput?.Invoke();
		}
	}

	public void OnReload(InputAction.CallbackContext context)
	{
		if (context.performed)
			OnReloadInput?.Invoke();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnJumpBeginInput?.Invoke();
		}
		else if (context.canceled)
		{
			OnJumpEndInput?.Invoke();
		}
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		Vector2 value = context.ReadValue<Vector2>();
		OnMoveInput?.Invoke(value);
	}
}
