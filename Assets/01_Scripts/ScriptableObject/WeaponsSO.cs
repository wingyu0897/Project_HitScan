using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/Weapons")]
public class WeaponsSO : ScriptableObject
{
    public List<WeaponDataSO> Weapons;

	private void OnEnable()
	{
		
	}
}
