using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoSingleton<UIManager>
{
    public static UIViewManager UIViewManager => _uiViewmanager;
    private static UIViewManager _uiViewmanager;
    public static SceneUIViewManager SceneUIViewManager => _sceneUIViewManager;
    private static SceneUIViewManager _sceneUIViewManager;

	protected override void Awake()
	{
		base.Awake();
		SceneManager.sceneLoaded += HandleSceneLoaded;
		_sceneUIViewManager = FindObjectOfType<SceneUIViewManager>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		SceneManager.sceneLoaded -= HandleSceneLoaded;
	}

	private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		_uiViewmanager = GameObject.Find("Canvas")?.GetComponent<UIViewManager>();
	}
}
