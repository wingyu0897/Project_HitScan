using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayView : UIView
{
    [Header("Weapon Info")]
    [SerializeField] private TextMeshProUGUI _currentAmmoTxt;
    [SerializeField] private TextMeshProUGUI _maxAmmoTxt;
    [SerializeField] private Image _weaponImage;

    [Header("Health Info")]
    [SerializeField] private TextMeshProUGUI _currentHealthTxt;
    [SerializeField] private Slider _healthSlider;

    [Header("Game Mode")]
    private Dictionary<GAME_MODE, GameModeUI> _gameModeUI = new();
    private GAME_MODE _currentGameMode;

	protected override void Awake()
	{
        base.Awake();

		foreach (GAME_MODE gameMode in System.Enum.GetValues(typeof(GAME_MODE)))
		{
            GameModeUI gameModeUI = transform.Find("GameModeUIs").Find(gameMode.ToString()).GetComponent<GameModeUI>();
            _gameModeUI[gameMode] = gameModeUI;
            gameModeUI.gameObject.SetActive(false);
        }
	}

	#region Weapon
	public void InitWeaponData(int maxAmmo, Sprite weaponSprite)
	{
        _weaponImage.sprite = weaponSprite;
        _maxAmmoTxt.text = _currentAmmoTxt.text = maxAmmo.ToString();
	}

    public void SetCurrentAmmo(int currentAmmo)
	{
        _currentAmmoTxt.text = currentAmmo.ToString();
	}
	#endregion

	#region Health
	public void InitHealthData(int maxHealth)
	{
        _healthSlider.value = 1.0f;
        _currentHealthTxt.text = maxHealth.ToString();
	}

    public void SetHealth(int maxHealth, int currentHealth)
	{
        _healthSlider.value =  (float)currentHealth / maxHealth;
        _currentHealthTxt.text = currentHealth.ToString();
	}
	#endregion

	#region GameModeUI
    public void SetIntermission()
	{

	}

	public void SetGameModeUI(InGameManager inGameMng, GAME_MODE gameMode)
	{
        inGameMng.OnGameTimerCount += HandleOnGameTimerCount;
        inGameMng.OnScoreChanged += HandleOnScoreChanged;
        _currentGameMode = gameMode;

        foreach (var gameModeUI in _gameModeUI)
		{
            gameModeUI.Value.gameObject.SetActive(gameModeUI.Key == _currentGameMode);
		}
    }

	private void HandleOnGameTimerCount(int leftTime)
	{
        int leftMinutes = leftTime / 60;
        int leftSeconds = leftTime % 60;
        _gameModeUI[_currentGameMode].SetTime(string.Format("{0:0}:{1:00}", leftMinutes, leftSeconds));
	}

	private void HandleOnScoreChanged(int redScore, int blueScore)
	{
        Debug.Log($"Score Changed. Red: {redScore} Blue: {blueScore}");
        _gameModeUI[_currentGameMode].SetScore(redScore.ToString(), blueScore.ToString());
	}
    #endregion
}
