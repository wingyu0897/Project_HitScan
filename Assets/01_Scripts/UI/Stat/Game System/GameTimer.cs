using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _timeTxt;

	private void Start()
	{
		BattleManager.Instance.OnGameTimerCount += HandleOnGameTimerCount;
	}

	private void OnDestroy()
	{
		if (!BattleManager.InstanceIsNull)
			BattleManager.Instance.OnGameTimerCount -= HandleOnGameTimerCount;
	}

	private void HandleOnGameTimerCount(int time)
	{
		int leftMinutes = time / 60;
		int leftSeconds = time % 60;
		_timeTxt?.SetText(string.Format("{0:00}:{1:00}", leftMinutes, leftSeconds));
	}
}
