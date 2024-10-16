using System.Collections;
using TMPro;
using UnityEngine;

public class Message : MonoBehaviour
{
	public TextMeshProUGUI TextMesh;
	public float AppearTime = 0.1f;
	public float Duration = 2f;
	public float FadeTime = 1f;
	
	private float _timer = 0;
	private Color _color;

	private void OnEnable()
	{
		_color = TextMesh.color;

		StopAllCoroutines();
		StartCoroutine(DurationCo());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		Destroy(gameObject);
	}

	IEnumerator DurationCo()
	{
		_timer = 0;

		while (_timer < AppearTime) {
			_timer += Time.deltaTime;
			transform.localScale = new Vector3(1, _timer / AppearTime);
			yield return null;
		}
		transform.localScale = Vector3.one;

		yield return new WaitForSeconds(Duration);

		_timer = FadeTime;
		while (_timer > 0) {
			_timer -= Time.deltaTime;
			_color.a = _timer / FadeTime;

			TextMesh.color = _color;

			yield return null;
		}

		Destroy(gameObject);
	}
}
