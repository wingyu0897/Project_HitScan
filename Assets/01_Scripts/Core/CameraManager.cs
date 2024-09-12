using Cinemachine;
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

	public void SetCameraTarget(Transform target)
	{
		_playerCam.Follow = target;
	}

    public void SetCamera(CAMERA_TYPE type)
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
}
