using TMPro;
using UnityEngine;

public class DeathView : UIView
{
    [SerializeField] private TextMeshProUGUI _killerText;

    public void SetKillerText(string userName)
	{
		_killerText.text = userName;
	}
}
