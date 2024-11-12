using UnityEngine;

public class PoolParticle : MonoBehaviour
{
	private PoolManager<PoolParticle> _pool;

	public void SetPool(PoolManager<PoolParticle> pool)
	{
		_pool = pool;
	}

	private void OnParticleSystemStopped()
	{
		if (_pool == null)
		{
			Destroy(gameObject);
			return;
		}	

		_pool.Pool.Release(this);
	}
}
