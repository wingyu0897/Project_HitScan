using TMPro;
using UnityEngine;

public class GameModeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _redScoreText;
    [SerializeField] private TextMeshProUGUI _blueScoreText;

    public void SetTime(string time)
	{
        if (_timeText != null)
            _timeText.text = time;
	}

    public void SetScore(string red, string blue)
	{
        _redScoreText.text = red;
        _blueScoreText.text = blue;
	}
}
