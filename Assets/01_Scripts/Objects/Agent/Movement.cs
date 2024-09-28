using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
	[SerializeField] private float _moveSpeed = 5f;
	[SerializeField] private float _jumpForce = 5f;
	[SerializeField] private float _coyoteTime = 0.1f;

	private Rigidbody2D _rigid;

	private Vector2 _moveDir;
	private bool _isGrounded = false;
	private float _coyoteTimer;

	private void Awake()
	{
		_rigid = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		if (!IsOwner) return;

		CheckGrounded();
	}

	private void FixedUpdate()
	{
		if (!IsOwner) return;

		Move();
	}

	public void SetMove(Vector2 dir)
	{
		_moveDir.x = dir.x * _moveSpeed;
	}

	private void Move()
	{
		_rigid.velocity = new Vector2(_moveDir.x, _rigid.velocity.y);

		_moveDir.x = 0;
	}

	public void Jump()
	{
		if (_isGrounded || _coyoteTimer > 0f)
		{
			_rigid.velocity = new Vector2(_rigid.velocity.x, _jumpForce);
			_coyoteTimer = 0;
		}
	}

	private void CheckGrounded()
	{
		bool prevIsGrounded = _isGrounded;
		_isGrounded = Physics2D.OverlapBox(transform.position - new Vector3(0, 0.5f), new Vector2(0.8f, 0.02f), 0, 1 << LayerMask.NameToLayer("Ground"));

		if (prevIsGrounded && !_isGrounded)
			_coyoteTimer = _coyoteTime;

		if (!_isGrounded)
			_coyoteTimer = Mathf.Clamp(_coyoteTimer - Time.deltaTime, 0, _coyoteTime);
	}
}
