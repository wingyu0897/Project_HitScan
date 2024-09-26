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
    [SerializeField] private GameObject _teamDeathMatch;

    public void InitWeaponData(int maxAmmo, Sprite weaponSprite)
	{
        _weaponImage.sprite = weaponSprite;
        _maxAmmoTxt.text = _currentAmmoTxt.text = maxAmmo.ToString();
	}

    public void SetCurrentAmmo(int currentAmmo)
	{
        _currentAmmoTxt.text = currentAmmo.ToString();
	}

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

    public void SetGameModeUI(GAME_MODE gameMode)
	{

	}
}
