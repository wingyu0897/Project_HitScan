using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FlipVisual : MonoBehaviour
{
	private SpriteRenderer _spriteRenderer;

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		_spriteRenderer.flipX = mousePos.x < transform.position.x;
	}
}
