using UnityEngine;
using UnityEngine.U2D.Animation;

public class WeaponAnimator : MonoBehaviour
{
	[SerializeField] private PoolParticle _bulletShellEffect;
	private PoolManager<PoolParticle> _bulletPoolManager;

    private Animator _animator;
	private SpriteLibrary _spriteLibrary;

    private readonly int IdleHash = Animator.StringToHash("Idle");
    private readonly int FireHash = Animator.StringToHash("Fire");
    private readonly int ReloadHash = Animator.StringToHash("Reload");

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_spriteLibrary = GetComponent<SpriteLibrary>();

		_bulletPoolManager = new PoolManager<PoolParticle>(_bulletShellEffect, 30, 100);
	}

	public void ChangeVisual(SpriteLibraryAsset asset)
	{
		_spriteLibrary.spriteLibraryAsset = asset;
	}

	public void Idle()
	{
		_animator.SetTrigger(IdleHash);
	}

	public void Fire()
	{
		_animator.SetTrigger(FireHash);

		PoolParticle bullet = _bulletPoolManager.Pool.Get();
		bullet.SetPool(_bulletPoolManager);
		bullet.transform.position = transform.position;

		//Instantiate(_bulletShellEffect, transform.position, Quaternion.identity);
	}

	public void Reload()
	{
		_animator.SetTrigger(ReloadHash);
	}

	public void SetSpeed(float speed)
	{
		_animator.speed = speed;
	}

	public void WeaponAnimationTrigger()
	{

	}

	private void OnDestroy()
	{
		_bulletPoolManager.Dispose();
	}
}
