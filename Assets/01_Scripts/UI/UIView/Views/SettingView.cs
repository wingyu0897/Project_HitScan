using UnityEngine;

public class SettingView : UIView
{
    [SerializeField] private UnityEngine.UI.Button _backButton;

    private void Awake()
	{
		_backButton.onClick.AddListener(Hide);
	}
}
