using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponPanelData : IPanelData
{
    public Sprite WeaponImage;
    public string WeaponName;
	public Action ClickAction;
}

public class WeaponSelectPanel : UIPanel, IPointerClickHandler
{
	public Image WeaponImage;
	public TextMeshProUGUI WeaponText;

	private Action _callback;

	public override void BindData(IPanelData data)
	{
		WeaponPanelData weaponData = data as WeaponPanelData;
		if (weaponData != null)
		{
			WeaponImage.sprite = weaponData.WeaponImage;
			WeaponText.SetText(weaponData.WeaponName);
			_callback = weaponData.ClickAction;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_callback?.Invoke();
	}
}
