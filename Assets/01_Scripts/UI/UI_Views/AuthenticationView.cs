using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationView : UIView
{
    [SerializeField] private TMP_InputField _nameField;
    [SerializeField] private Button _playBtn;

	protected override void Awake()
	{
		base.Awake();

		_playBtn.onClick.AddListener(StartAuthentication);
		LobbyManager.Instance.OnSignedIn += OnSignedInHandler;
	}

	private void OnSignedInHandler(object e, System.EventArgs args)
	{
		UIViewManager.Instance.HideView(this);
		UIViewManager.Instance.ShowView<LobbyView>();
	}

	private void StartAuthentication()
	{
		if (string.IsNullOrWhiteSpace(_nameField.text) || string.IsNullOrEmpty(_nameField.text))
			return;

		LobbyManager.Instance.Authenticate(_nameField.text);
	}
}
