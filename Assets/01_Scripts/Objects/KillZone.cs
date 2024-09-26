using Unity.Netcode;
using UnityEngine;

public class KillZone : NetworkBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!IsHost) return;

		if (collision.attachedRigidbody != null && collision.attachedRigidbody.TryGetComponent(out PlayerAgent player))
		{
			player.Health.SetDealer(256);
			player.Kill();
		}
	}
}
