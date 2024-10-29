using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    protected AudioSource _audioSource;

	protected float _defaultPitch;
	protected bool _setDefaultAfterPlay;

	protected virtual void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		_defaultPitch = _audioSource.pitch;
	}

	public virtual void PlayAudio(AudioClip clip)
	{
		if (clip == null) return;

		_audioSource.Stop();
		_audioSource.clip = clip;
		_audioSource.Play();

		if (_setDefaultAfterPlay)
			_audioSource.pitch = _defaultPitch;
	}

	public void SetRandomPitch(float randomness, bool setDefaultAfterPlay = true)
	{
		_setDefaultAfterPlay = setDefaultAfterPlay;
		randomness = Mathf.Clamp01(randomness);

		float pitch = _audioSource.pitch;
		_audioSource.pitch = pitch * Random.Range(-pitch * randomness, pitch * randomness);
	}

	public void SetDistance(float distance)
	{
		_audioSource.maxDistance = distance;
	}
}
