using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIViewManager : MonoSingleton<UIViewManager>
{
	private List<UIView> _views = new List<UIView>();

	[SerializeField] private UIView _defaultView;

	protected override void Awake()
	{
		base.Awake();

		GetComponentsInChildren<UIView>(true, _views);
	}

	private void Start()
	{
		if (_defaultView != null)
			ShowView(_defaultView);
		else if (_views.Count > 0)
			ShowView(_views[0]);	
	}

	public T GetScene<T>() where T : UIView
	{
		T view = _views.Find(v => v.GetType() == typeof(T)) as T;
		return view;
	}

	public void ShowView<T>() where T : UIView
	{
		UIView view = _views.Find(view => view is T);

		if (view != null)
			view.Show();
	}

	public void ShowView(UIView view)
	{
		if (view != null)
			view.Show();
	}

	public void HideView<T>() where T : UIView
	{
		UIView view = _views.Find(view => view is T);

		if (view != null)
			view.Hide();
	}

	public void HideView(UIView view)
	{
		if (view != null)
			view.Hide();
	}
}
