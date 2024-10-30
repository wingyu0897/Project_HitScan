using TMPro;
using UnityEngine;

public class LoadingView : UIView
{
    [SerializeField] private TextMeshProUGUI _loadingTxt;

    public void SetText(string text)
	{
		_loadingTxt.SetText(text);
	}
}
