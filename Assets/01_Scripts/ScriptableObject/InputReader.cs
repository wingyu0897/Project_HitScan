using UnityEngine;

public class InputReader : ScriptableObject
{
	protected InputSystem input;

	protected virtual void OnEnable()
	{
		if (input == null)
		{
			input = new InputSystem();
		}

		input.Player.Enable();
		input.UI.Enable();
	}
}
