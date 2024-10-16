using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIViewManager : MonoBehaviour
{
	protected Dictionary<Type, UIView> _viewsByType = new Dictionary<Type, UIView>();

	[SerializeField] protected UIView _defaultView;

	protected virtual void Awake()
	{
		foreach (UIView view in GetComponentsInChildren<UIView>(true))
		{
			_viewsByType.Add(view.GetType(), view);
		}
	}

	protected virtual void Start()
	{
		if (_defaultView != null)
			ShowView(_defaultView);
		else
			Debug.Log("기본 뷰가 존재하지 않음.");
	}

	public virtual T GetView<T>() where T : UIView
	{
		T view = _viewsByType[typeof(T)] as T;
		return view;
	}

	public virtual void ShowView<T>() where T : UIView
	{
		if (_viewsByType.ContainsKey(typeof(T)))
		{
			UIView view = _viewsByType[typeof(T)];
			view.Show();
		}
	}

	public virtual void ShowView(UIView view)
	{
		if (view != null)
			view.Show();
	}

	public virtual void HideView<T>() where T : UIView
	{
		if (_viewsByType.ContainsKey(typeof(T)))
		{
			UIView view = _viewsByType[typeof(T)];
			view.Hide();
		}
	}

	public virtual void HideView(UIView view)
	{
		if (view != null)
			view?.Hide();
	}
}
