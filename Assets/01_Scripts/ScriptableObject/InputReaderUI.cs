using System;
using UnityEngine;

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

	public void OnClick(UnityEngine.InputSystem.InputAction.CallbackContext context)
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
}
