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
		{
			
			gameObject.layer = LayerMask.NameToLayer("Owner");
			transform.Find("Visual").gameObject.layer = LayerMask.NameToLayer("Owner");
		}
		else
		{
			gameObject.layer = LayerMask.NameToLayer("Player");
			transform.Find("Visual").gameObject.layer = LayerMask.NameToLayer("Player");
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetColorServerRpc()
	{
		if (_spriteRenderer.color == Color.white)
			ApplyColorClientRpc(Random.ColorHSV(1f, 1f, 0f, 1f));
		else
			ApplyColorClientRpc(_spriteRenderer.color);
	}

	[ClientRpc]
	private void ApplyColorClientRpc(Color color)
	{
		_spriteRenderer.color = color;
	}
}
