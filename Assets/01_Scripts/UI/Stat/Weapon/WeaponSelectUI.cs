using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSelectUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private InputReaderUI _inputReader;

	[Header("Selection List Panel")]
	[SerializeField] private GameObject _weaponList;
	[SerializeField] private Transform _container;
	[SerializeField] private UIPanel _panelPrefab;

	[Header("Selected Panel")]
    [SerializeField] private Image _weaponImage;
	[SerializeField] private TextMeshProUGUI _weaponName;

	private bool _isPointerOverUI = false;
	private ContentManager _contentManager = new ContentManager();

	public event Action<WeaponDataSO> OnWeaponChanged;

	private void Awake()
	{
		ShowList(false);

		_inputReader.OnPointerUp += () =>
		{
			if (_weaponList == null) return;
			if (_weaponList.activeInHierarchy && !_isPointerOverUI)
				ShowList(false);
		};
	}

	public void CreateList(WeaponsSO weapons)
	{
		for (int i = 0; i < weapons.Weapons.Count; ++i)
		{
			int index = i;
			WeaponPanelData panelData = new WeaponPanelData() { 
				WeaponImage = weapons.Weapons[i].Visual, 
				WeaponName = weapons.Weapons[i].Name,
				ClickAction = () => ChangeWeapon(weapons.Weapons[index]),
			};
			_contentManager?.CreatePanel(_container, _panelPrefab, panelData);
		}
	}

	public void SetVisual(WeaponDataSO data)
	{
		_weaponImage.sprite = data.Visual;
		_weaponName.SetText(data.Name);
	}

	public void ChangeWeapon(WeaponDataSO data)
	{
		OnWeaponChanged?.Invoke(data);
		ShowList(false);
	}

	private void ShowList(bool show)
	{
		_weaponList?.SetActive(show);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		ShowList(true);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isPointerOverUI = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isPointerOverUI = false;
	}
}
