using System.Collections;
using TMPro;
using UnityEngine;

public class KillLogPanelUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _attackText;
	[SerializeField] private TextMeshProUGUI _deadText;

	private float _duration;
	private float _fadeTime;

	private void OnDisable()
	{
		StopAllCoroutines();
		Destroy(gameObject);
	}

	public void Init(string attack, Color attackColor, string dead, Color deadColor, float duration = 1f, float fadeTime = 1f)
	{
		_attackText.text = attack;
		_attackText.color = attackColor;
		_deadText.text = dead;
		_deadText.color = deadColor;

		_duration = duration;
		_fadeTime = fadeTime;

		/* ���� Ǯ���� �� �����ϵ��� ����. �����غ��� Init�� ���� ����Ǿ��� �� ���⵵ �ѵ�, 
		 * Ǯ���� �� poolinit�ϱ� ���� �ν��Ͻ��� �ִ� ����̳� �� �������� ����� ������ */

		// ĵ������ ��Ȱ��ȭ �Ǿ����� ���� �������� ����
		if (gameObject.activeInHierarchy)
			StartCoroutine(FadeCo());
		else
			Destroy(gameObject);
	}

	IEnumerator FadeCo()
	{
		yield return new WaitForSeconds(_duration);

		Destroy(gameObject); // Ǯ������ �����ؾ� ��
		// ���̵� �ƿ��ϴ� �� �����ؾ� ��
	}
}
