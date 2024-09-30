using Cinemachine;
using System;
using System.Collections;
using Unity.Netcode;
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
	private float _originOrthoSize;
	private bool _isAiming = false;

	private Coroutine _orthoSizeCo = null;
	private Coroutine _deathCamCo = null;

	public void SetCameraTarget(Transform target)
	{
		_playerCam.Follow = target;
		_transposer = _playerCam.GetCinemachineComponent<CinemachineFramingTransposer>();
		_originOrthoSize = _playerCam.m_Lens.OrthographicSize;
	}

    public void SetCameraType(CAMERA_TYPE type)
	{
		switch (type)
		{
			case CAMERA_TYPE.Player:
				_mapCam.Priority = 0;
				_playerCam.Priority = 1;
				break;
			case CAMERA_TYPE.Map:
				_mapCam.Priority = 1;
				_playerCam.Priority = 0;
				break;
		}
	}

	/// <summary>
	/// 잠시동안 카메라 위치를 자신을 죽인 적으로 변경한다
	/// </summary>
	public void DeathCam(ulong attackerId, float duration = 1.0f, Action callback = null)
	{
		SetCameraTarget(Players.GetPlayerObjectByClientID(attackerId)?.transform);

		if (_deathCamCo != null)
			StopCoroutine(_deathCamCo);
		_deathCamCo = StartCoroutine(DeathCamCo(duration, callback));
	}

	IEnumerator DeathCamCo(float duration, Action callback)
	{
		yield return new WaitForSeconds(duration);

		callback?.Invoke();
		_deathCamCo = null;
	}

	public void AimCamera(Vector2 offset, float orthoSizeRatio = 1f, float lerpTime = 0.3f)
	{
		if (_transposer == null) return;

		_transposer.m_TrackedObjectOffset = offset;

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
}
