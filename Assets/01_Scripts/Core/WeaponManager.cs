using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponsSO _weaponDatas;

    private WeaponDataSO _weapon;
	public string WeaponName {
		get
		{
			if (_weapon == null)
				return "Sample";
			return _weapon.Name;
		}
    }

	private void Start()
	{
		UIManager.UIViewManager.GetView<GameReadyView>().WeaponSelect.CreateList(_weaponDatas); // ���� �����͸� �������� ���� ������ ���� UI ����
		UIManager.UIViewManager.GetView<GameReadyView>().WeaponSelect.OnWeaponChanged += WeaponChangedHandle;

		if (_weapon != null)
			ChangeWeapon(_weapon.Name);
		else
			ChangeWeapon(_weaponDatas.Weapons[0]);
	}

	public void ChangeWeapon(WeaponDataSO data)
	{
		if (data != null)
		{
			_weapon = data;
			UIManager.UIViewManager.GetView<GameReadyView>().WeaponSelect?.SetVisual(data);
		}
	}

	public void ChangeWeapon(string weaponName)
	{
		WeaponDataSO data = _weaponDatas.Weapons.Find(n => n.Name == weaponName);
		ChangeWeapon(data);
	}

	/// <summary>
	/// UI���� ���Ⱑ ���õǾ��� ���
	/// </summary>
	private void WeaponChangedHandle(WeaponDataSO data)
	{
		ChangeWeapon(data);
	}
}
