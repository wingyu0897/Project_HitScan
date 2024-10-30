using UnityEngine;
using UnityEngine.Audio;

public class SettingManager : MonoSingleton<SettingManager>
{
	[SerializeField] private SettingValue[] _settingValues;

	[SerializeField] private AudioMixer _masterAudioMixer;

	private void OnEnable()
	{
		foreach (SettingValue sv in _settingValues)
		{
			sv.OnValueChanged += HandleSettingChanged;
		}
	}

	private void OnDisable()
	{
		foreach (SettingValue sv in _settingValues)
		{
			sv.OnValueChanged -= HandleSettingChanged;
		}
	}

	private void HandleSettingChanged(Define.SETTING_TYPE settingType, ISettingData settingData)
	{
		switch (settingType)
		{
			case Define.SETTING_TYPE.BGM:
			case Define.SETTING_TYPE.SFX:
				VolumeSetting.VolumeSettingData vs = settingData as VolumeSetting.VolumeSettingData;
				ChangeVolume(settingType, vs.Volume);
				break;
		}
	}

	public void ChangeVolume(Define.SETTING_TYPE mixerType, float volume)
	{
		switch (mixerType)
		{
			case Define.SETTING_TYPE.BGM:
				_masterAudioMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
				break;
			case Define.SETTING_TYPE.SFX:
				_masterAudioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
				break;
		}
	}
}
