using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private int _fireRate;
	[SerializeField] private float _distance = 50f;

    public ulong ClientId;

	public virtual void Attack()
	{
		int layer = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Default"));
		RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, _distance, layer);

		if (hit.collider != null)
			//if (TryGetComponent(out Health health))
			{
				//health.
			}
	}
}
