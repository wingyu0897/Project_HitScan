using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIViewManager : MonoBehaviour
{
	protected Dictionary<Type, UIView> _views = new Dictionary<Type, UIView>();

	[SerializeField] protected UIView _defaultView;

	protected virtual void Awake()
	{
		foreach (UIView view in GetComponentsInChildren<UIView>(true))
		{
			_views.Add(view.GetType(), view);
		}
	}

	protected virtual void Start()
	{
		foreach (var view in _views) {
			view.Value.Hide();
		}

		if (_defaultView != null)
			_defaultView.Show();
		else
			Debug.Log("기본 뷰가 존재하지 않음.");
	}

	public virtual T GetView<T>() where T : UIView
	{
		T view = _views[typeof(T)] as T;
		return view;
	}

	public virtual void ShowView<T>() where T : UIView
	{
		if (_views.ContainsKey(typeof(T)))
		{
			UIView view = _views[typeof(T)];
			view?.Show();
		}
	}

	public virtual void ShowView(UIView view)
	{
		if (view != null)
			view?.Show();
	}

	public virtual void HideView<T>() where T : UIView
	{
		if (_views.ContainsKey(typeof(T)))
		{
			UIView view = _views[typeof(T)];
			view?.Hide();
		}
	}

	public virtual void HideView(UIView view)
	{
		if (view != null)
			view?.Hide();
	}
}
