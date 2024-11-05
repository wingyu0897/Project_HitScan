using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "InputReaderUI")]
public class InputReaderUI : InputReader, InputSystem.IUIActions
{
	protected override void OnEnable()
	{
		base.OnEnable();

		input.UI.AddCallbacks(this);
	}

	public Action OnPointerDown;
	public Action OnPointerUp;
	public Action OnPointerUpButton;
	public Action OnTapDown;
	public Action OnTapUp;

	public void OnClick(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnPointerDown?.Invoke();
			return;
		}

		if (context.canceled)
		{
			//if (context.)

			OnPointerUp?.Invoke();
			return;
		}
	}

	public void OnTap(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			OnTapDown?.Invoke();
			return;
		}

		if (context.canceled)
		{
			OnTapUp?.Invoke();
			return;
		}
	}
}
