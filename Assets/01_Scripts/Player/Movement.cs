using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
	[SerializeField] private float moveSpeed = 5f;

	private void Update()
	{
		if (!IsOwner) return;

		Vector3 moveDir = Vector3.zero;

		if (Input.GetKey(KeyCode.W)) moveDir += Vector3.up;
		if (Input.GetKey(KeyCode.S)) moveDir -= Vector3.up;
		if (Input.GetKey(KeyCode.D)) moveDir += Vector3.right;
		if (Input.GetKey(KeyCode.A)) moveDir -= Vector3.right;

		transform.position += moveDir * moveSpeed * Time.deltaTime;
	}
}
