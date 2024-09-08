using UnityEngine;

public class UIView : MonoBehaviour
{
	protected virtual void Awake()
	{
		Hide();
	}

	public virtual void Show()
	{
		gameObject.SetActive(true);
	}

    public virtual void Hide()
	{
		gameObject.SetActive(false);
	}
}
