

using UnityEngine;

[System.Serializable]
public abstract class SettingValue : MonoBehaviour
{
	public delegate void SettingChange(Define.SETTING_TYPE settingType, ISettingData settingData);
	public abstract event SettingChange OnValueChanged;
}
