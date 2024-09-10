using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerAgent : NetworkBehaviour
{
	[SerializeField] private SpriteRenderer _spriteRenderer;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		SetColorServerRpc();

		if (IsOwner)
			Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera.Follow = transform;
	}

	[ClientRpc]
	public void SetLayerClientRpc(int layer)
	{
		gameObject.layer = layer;
		transform.Find("Visual").gameObject.layer = layer;
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetColorServerRpc()
	{
		if (_spriteRenderer.color == Color.white)
			ApplyColorClientRpc(Random.ColorHSV(0, 1f, 0, 1f, 0, 1f));
		else
			ApplyColorClientRpc(_spriteRenderer.color);
	}

	[ClientRpc]
	private void ApplyColorClientRpc(Color color)
	{
		_spriteRenderer.color = color;
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
	}
}
