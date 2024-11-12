using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

public enum CAMERA_TYPE
{
    Player,
    Map,
}

public class CameraManager : MonoSingleton<CameraManager>
{
    [SerializeField] private CinemachineVirtualCamera _mapCam;
    [SerializeField] private CinemachineVirtualCamera _playerCam;

	private CinemachineFramingTransposer _transposer;
	private CinemachineBasicMultiChannelPerlin _perlin;
	private float _originOrthoSize;
	private Vector2 _aimOffset;
	private bool _isAiming = false;

	private Coroutine _orthoSizeCo = null;
	private Coroutine _deathCamCo = null;
	private Coroutine _shakeCo = null;

	public void SetCameraTarget(Transform target)
	{
		_playerCam.Follow = target;
		_transposer = _playerCam.GetCinemachineComponent<CinemachineFramingTransposer>();
		_perlin = _playerCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
		_originOrthoSize = _playerCam.m_Lens.OrthographicSize;
	}

    public void SetCameraType(CAMERA_TYPE type)
	{
		switch (type)
		{
			case CAMERA_TYPE.Player:
				_mapCam.enabled = false;
				_playerCam.enabled = true;
				_mapCam.Priority = 0;
				_playerCam.Priority = 1;
				break;
			case CAMERA_TYPE.Map:
				_mapCam.enabled = true;
				_playerCam.enabled = false;
				_mapCam.Priority = 1;
				_playerCam.Priority = 0;
				break;
		}
	}

	public void ShakeCam(float intensity, float time)
	{
		if (_shakeCo != null)
			StopCoroutine(_shakeCo);
		_shakeCo = StartCoroutine(ShakeCo(intensity, time));
	}

	IEnumerator ShakeCo(float intensity, float time)
	{
		_perlin.m_AmplitudeGain = intensity;

		yield return new WaitForSeconds(time);

		_perlin.m_AmplitudeGain = 0;
	}

	/// <summary>
	/// 잠시동안 카메라 위치를 자신을 죽인 적으로 변경한다
	/// </summary>
	public void DeathCam(ulong attackId, float duration = 1.0f, Action callback = null)
	{
		if (_deathCamCo != null)
			StopCoroutine(_deathCamCo);
		_deathCamCo = StartCoroutine(DeathCamCo(Players.GetPlayerObjectByClientID(attackId)?.transform, duration, callback));
	}

	IEnumerator DeathCamCo(Transform target, float duration, Action callback)
	{
		if (target != null)
		{
			yield return new WaitForSeconds(1f);

			SetCameraTarget(target);
		}

		yield return new WaitForSeconds(duration);

		callback?.Invoke();
		_deathCamCo = null;
	}

	public void AimCamera(Vector2 offset, float orthoSizeRatio = 1f, float lerpTime = 0.3f)
	{
		if (_transposer == null) return;

		_transposer.m_TrackedObjectOffset = offset + _aimOffset;

		if (_isAiming == (orthoSizeRatio == 1f))
		{
			_isAiming = !_isAiming;
			if (_orthoSizeCo != null)
				StopCoroutine(_orthoSizeCo);

			_orthoSizeCo = StartCoroutine(OrthoSizeCo(lerpTime, _playerCam.m_Lens.OrthographicSize, _originOrthoSize * orthoSizeRatio));
		}
	}

	IEnumerator OrthoSizeCo(float time, float start, float target)
	{
		float timer = 0;

		while (timer < time)
		{
			timer += Time.deltaTime;
			_playerCam.m_Lens.OrthographicSize = Mathf.Lerp(start, target, timer / time);

			yield return null;
		}

		_playerCam.m_Lens.OrthographicSize = target;

		_orthoSizeCo = null;
	}
	public void SetAimOffset(Vector3 offset)
	{
		_aimOffset = offset;
		if (!_isAiming)
			_transposer.m_TrackedObjectOffset = offset;
	}
}
