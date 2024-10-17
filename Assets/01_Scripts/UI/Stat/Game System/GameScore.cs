using TMPro;
using UnityEngine;

public class GameScore : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _redScoreText;
	[SerializeField] private TextMeshProUGUI _blueScoreText;

	private void Awake()
	{
		BattleManager.Instance.OnScoreChanged += HandleOnScoreChanged;
	}

	private void OnDestroy()
	{
		if (!BattleManager.InstanceIsNull)
			BattleManager.Instance.OnScoreChanged -= HandleOnScoreChanged;
	}

	private void HandleOnScoreChanged(int redScore, int blueScore)
	{
		_redScoreText.text = redScore.ToString();
		_blueScoreText.text = blueScore.ToString();
	}
}
