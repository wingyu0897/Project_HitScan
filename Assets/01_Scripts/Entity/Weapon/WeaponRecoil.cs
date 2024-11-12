using Define;
using System.Collections;
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [SerializeField] private Transform recoilTrm;
	[SerializeField] private float maxAngle = 90f;
	[SerializeField] private float maxDistance;
	[SerializeField] private float recoilTime = 0.1f;
	[SerializeField] private float recoveryTime = 1f;

	private float originX;

	private Coroutine recoilCo = null;

	private void Awake()
	{
		originX = recoilTrm.localPosition.x;
	}

	public void DoRecoil(float angle, float distance)
	{
		// angle
		float currentAngle = recoilTrm.localEulerAngles.z;
		if (currentAngle > 180)
			currentAngle = Mathf.Abs(currentAngle - 360);
		else if (currentAngle < 1f)
			currentAngle = 0f;

		float targetAngle = Random.Range(0, angle + currentAngle);
		if (currentAngle != 0 && currentAngle < 180)
			targetAngle *= -1;
		targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);

		// X
		float currentX = recoilTrm.localPosition.x;
		float targetX = Mathf.Clamp(currentX - distance, -maxDistance, originX);

		if (recoilCo != null)
			StopCoroutine(recoilCo);
		recoilCo = StartCoroutine(RecoilCo(currentAngle, targetAngle, currentX, targetX));

		// Camera
		CameraManager.Instance.ShakeCam(2f, 0.1f);
	}

	IEnumerator RecoilCo(float startAngle, float targetAngle, float startX, float targetX)
	{
		float timer = 0;
		Vector3 mousePos;
		Vector3 dir;

		while (timer < recoilTime)
		{
			mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			dir = transform.position - mousePos;

			float per = timer / recoilTime;
			recoilTrm.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(startAngle, targetAngle, Ease.OutSuperExpo(0, 1f, per)));
			recoilTrm.localPosition = new Vector3(Ease.OutSuperExpo(startX, targetX, per), recoilTrm.localPosition.y, 0);
			CameraManager.Instance.SetAimOffset(dir.normalized * Ease.OutSuperExpo(0f, 2f, per));

			timer += Time.deltaTime;
			yield return null;
		}

		recoilTrm.localRotation = Quaternion.Euler(0, 0, targetAngle);
		recoilCo = StartCoroutine(Recovery());
	}

	IEnumerator Recovery()
	{
		float angle = recoilTrm.localEulerAngles.z;
		float x = recoilTrm.localPosition.x;
		float timer = 0;
		Vector3 mousePos;
		Vector3 dir;

		while (timer < recoveryTime)
		{
			mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			dir = transform.position - mousePos;

			float per = timer / recoilTime;
			recoilTrm.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(angle, 0, per));
			recoilTrm.localPosition = new Vector3(Ease.OutExpo(x, originX, per), recoilTrm.localPosition.y, 0);
			CameraManager.Instance.SetAimOffset(dir.normalized * Mathf.Lerp(2f, 0f, per));

			timer += Time.deltaTime;
			yield return null;
		}

		CameraManager.Instance.SetAimOffset(Vector3.zero);
		recoilTrm.localPosition = Vector3.zero;
		recoilTrm.localRotation = Quaternion.Euler(0, 0, 0);

		recoilCo = null;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		CameraManager.Instance.SetAimOffset(Vector3.zero);
		recoilTrm.localPosition = Vector3.zero;
		recoilTrm.localRotation = Quaternion.Euler(0, 0, 0);

	}
}
