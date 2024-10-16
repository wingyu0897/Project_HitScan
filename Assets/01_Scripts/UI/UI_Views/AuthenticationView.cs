using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationView : UIView
{
    [SerializeField] private TMP_InputField _nameField;
    [SerializeField] private Button _playBtn;
	[SerializeField] private GameObject _titleView;
	[SerializeField] private GameObject _authenticatingView;

	private Coroutine _authenticatingCo;

	private WaitForSeconds _wait = new WaitForSeconds(0.5f);

	private void Awake()
	{
		ShowTitle(true);

		_playBtn.onClick.AddListener(StartAuthentication);
		LobbyManager.Instance.OnSignedIn += HandleOnSignedIn;
		LobbyManager.Instance.OnSignInFailed += HandleOnSignInFailed;
	}

	private void OnDestroy()
	{
		if (!LobbyManager.InstanceIsNull)
		{
			LobbyManager.Instance.OnSignedIn -= HandleOnSignedIn;
			LobbyManager.Instance.OnSignInFailed -= HandleOnSignInFailed;
		}
	}

	private void HandleOnSignedIn(object e, System.EventArgs args)
	{
		ShowTitle(true);
		UIManager.UIViewManager.HideView(this);
		UIManager.UIViewManager.ShowView<LobbyView>();
	}

	private void HandleOnSignInFailed(object sender, EventArgs e)
	{
		ShowTitle(true);
	}

	private void StartAuthentication()
	{
		if (string.IsNullOrWhiteSpace(_nameField.text) || string.IsNullOrEmpty(_nameField.text))
			return;

		LobbyManager.Instance.Authenticate(_nameField.text);
		ShowTitle(false);
	}

	private void ShowTitle(bool show)
	{
		_titleView.SetActive(show);
		_authenticatingView.SetActive(!show);
	}
}
