using System;
using UnityEngine;
using UnityEngine.UI;

public class GameReadyView : UIView
{
	[Header("Play")]
	[SerializeField] private Button _playBtn;

	public event Action OnPlayButtonClick;

	protected override void Awake()
	{
		base.Awake();

		_playBtn.onClick.AddListener(HandlePlayButtonClick);
	}

	private void HandlePlayButtonClick()
	{
		Hide();
		OnPlayButtonClick?.Invoke();
		UIViewManager.Instance.ShowView<GamePlayView>();
	}
}
