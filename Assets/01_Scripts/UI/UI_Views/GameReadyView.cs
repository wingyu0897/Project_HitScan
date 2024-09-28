using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReadyView : UIView
{
	[Header("Play")]
	[SerializeField] private Button _playBtn;

	public event Action OnPlayButtonClick;

	[Header("Intermission")]
	[SerializeField] private GameObject _intermissionText;
	[SerializeField] private TextMeshProUGUI _intermissionTimeText;

	protected override void Awake()
	{
		base.Awake();

		_playBtn.onClick.AddListener(HandlePlayButtonClick);
	}

	private void HandlePlayButtonClick()
	{
		OnPlayButtonClick?.Invoke();
	}

	public void SetIntermission(bool active, int intermissionTime = 0)
	{
		_intermissionText.SetActive(active);
		_intermissionTimeText.gameObject.SetActive(active);
		_intermissionTimeText.text = intermissionTime.ToString();
	}
}
