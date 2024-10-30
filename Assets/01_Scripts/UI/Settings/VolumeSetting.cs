using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSetting : SettingValue
{
	public class VolumeSettingData : ISettingData
	{
		public float Volume;
	}

	public override event SettingChange OnValueChanged;
	private VolumeSettingData _volumeSettingData;


    [SerializeField] private Define.SETTING_TYPE _volumeType;
    [SerializeField] private Toggle _volumeToggle;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private TextMeshProUGUI _volumeText;


	private void Awake()
	{
		_volumeSlider.onValueChanged.AddListener(HandleSliderValueChanged);
		_volumeToggle.onValueChanged.AddListener(HandleToggleValueChanged);

		_volumeSettingData = new VolumeSettingData();
	}

	private void HandleToggleValueChanged(bool isOn)
	{
		if (!isOn) return;

		_volumeSlider.value = 0f;
		HandleSliderValueChanged(0f);
	}

	private void HandleSliderValueChanged(float value)
	{
		value = Mathf.Clamp(value, 0.0001f, 1f);
		if (value > 0.0001f)
			_volumeToggle.isOn = false;
		else
			_volumeToggle.isOn = true;

		_volumeText.SetText("{0}%", Mathf.FloorToInt(value * 100f));

		_volumeSettingData.Volume = value;
		OnValueChanged?.Invoke(_volumeType, _volumeSettingData);
	}
}
