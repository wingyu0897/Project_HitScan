using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
	[SerializeField] private float _moveSpeed = 5f;
	[SerializeField] private float _jumpForce = 5f;
	[SerializeField] private float _coyoteTime = 0.1f;

	private Rigidbody2D _rigid;

	private Vector2 _moveDir;

	private bool _isJumping = false;
	private bool _isGrounded = false;
	private float _coyoteTimer;
	private float _slowdown = 1f;

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
		Jump();
	}

	public void SetSlowdown(float slowdown)
	{
		_slowdown = 1f - Mathf.Clamp(slowdown, 0, 1f);
	}

	public void SetMove(Vector2 dir)
	{
		_moveDir.x = dir.x * _moveSpeed * _slowdown;
	}

	private void Move()
	{
		float yVelocity = _isGrounded && _rigid.velocity.y < 0 ? 0 : _rigid.velocity.y;
		_rigid.velocity = new Vector2(_moveDir.x, yVelocity);

		//_moveDir.x = 0;
	}

	public void SetJump(bool doJump)
	{
		_isJumping = doJump;
	}

	public void Jump()
	{
		if (!_isJumping) // ���� �� ��
			return;
		else if (!_isGrounded && _coyoteTimer <= 0f) // ���鿡 ���� ���� and �ڿ��� Ÿ�ӵ� �ƴ�
			return;

		_rigid.velocity = new Vector2(_rigid.velocity.x, _jumpForce * _slowdown);
		_coyoteTimer = 0;
	}

	private void CheckGrounded()
	{
		bool prevIsGrounded = _isGrounded; // ���鿡�� ���������� Ȯ���ϱ� ���ؼ� ���� �������� ����صд�
		RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(1f, 0.1f), 0f, Vector2.down, 0.45f, 1 << LayerMask.NameToLayer("Ground"));
		_isGrounded = hit.collider != null;

		if (prevIsGrounded && !_isGrounded) { // ���鿡�� �������� �Ǿ��ٸ� �ڿ��� Ÿ�� ���� ����
			_coyoteTimer = _coyoteTime;
		}

		if (_isGrounded) {
			transform.position = new Vector3(transform.position.x, hit.point.y + 0.5f);
		}
		else {
			_coyoteTimer = Mathf.Clamp(_coyoteTimer - Time.deltaTime, 0, _coyoteTime);
		}
	}
}
