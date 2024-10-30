using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoSingleton<UIManager>
{
	private Dictionary<System.Type, UIViewManager> _uiViewManagers = new Dictionary<System.Type, UIViewManager>();

	protected override void Awake()
	{
		base.Awake();
		SceneManager.sceneLoaded += HandleSceneLoaded;

	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		SceneManager.sceneLoaded -= HandleSceneLoaded;
	}

	private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		FindUIViewManagers();
	}

	private void FindUIViewManagers()
	{
		_uiViewManagers.Clear();

		UIViewManager[] uiViewManagers = FindObjectsByType<UIViewManager>(FindObjectsSortMode.None);

		foreach (UIViewManager uvm in uiViewManagers)
		{
			if (_uiViewManagers.ContainsKey(uvm.GetType())) return;

			_uiViewManagers.Add(uvm.GetType(), uvm);
		}
	}

	public static T Get<T>() where T : UIViewManager
	{
		if (_instance is null) return null;

		if (_instance._uiViewManagers.ContainsKey(typeof(T)))
			return _instance._uiViewManagers[typeof(T)] as T;

		return null;
	}
}
