using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameReadyView : UIView
{
	[Header("Loadout")]
	[SerializeField] private WeaponSelectUI _weaponSelect;
	public WeaponSelectUI WeaponSelect => _weaponSelect;

	[Header("GameMode")]
	[SerializeField] private TextMeshProUGUI _gameModeText;

	[Header("Buttons")]
	[SerializeField] private Button _playBtn;
	[SerializeField] private Button _leaveBtn;
	[SerializeField] private Button _settingBtn;

	public event Action OnPlayButtonClick;

	[Header("Intermission")]
	[SerializeField] private GameObject _intermissionText;
	[SerializeField] private TextMeshProUGUI _intermissionTimeText;

	private void Awake()
	{ 
		_playBtn.onClick.AddListener(HandlePlayButtonClick);
		_leaveBtn.onClick.AddListener(HandleLeaveButtonClick);
		_settingBtn.onClick.AddListener(HandleSettingButtonClick);

		//_userNameText.SetText(GameManager.Instance.UserName);

		BattleManager.Instance.OnGameStarted += HandleOnGameStarted;
	}

	private void HandlePlayButtonClick()
	{
		OnPlayButtonClick?.Invoke();
	}

	private void HandleLeaveButtonClick()
	{
		GameManager.Instance.LeaveRelayGame();
	}

	private void HandleSettingButtonClick()
	{
		UIManager.Get<SettingUIViewManager>().ShowView<SettingView>();
	}

	private void HandleOnGameStarted(GAME_MODE gameMode)
	{
		_gameModeText.SetText(gameMode.ToString());
	}

	public void SetIntermission(bool active, int intermissionTime = 0)
	{
		_intermissionText.SetActive(active);
		_intermissionTimeText.gameObject.SetActive(active);
		_intermissionTimeText.text = intermissionTime.ToString();
	}
}
