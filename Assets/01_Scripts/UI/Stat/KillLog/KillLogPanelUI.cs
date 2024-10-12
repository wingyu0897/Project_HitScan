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

		/* 추후 풀링될 때 실행하도록 변경. 생각해보니 Init이 먼저 실행되야할 것 같기도 한데, 
		 * 풀링할 때 poolinit하기 전에 인스턴스를 주는 방법이나 뭐 여러가지 고려해 봐야지 */

		// 캔버스가 비활성화 되어있을 때는 실행하지 않음
		if (gameObject.activeInHierarchy)
			StartCoroutine(FadeCo());
		else
			Destroy(gameObject);
	}

	IEnumerator FadeCo()
	{
		yield return new WaitForSeconds(_duration);

		Destroy(gameObject); // 풀링으로 변경해야 함
		// 페이드 아웃하는 거 제작해야 돼
	}
}
