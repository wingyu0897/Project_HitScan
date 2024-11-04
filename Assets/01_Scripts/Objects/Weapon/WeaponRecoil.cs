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
		float targetAngle = Random.Range(0, angle + currentAngle);
		if (recoilTrm.localEulerAngles.z != 0 && recoilTrm.localEulerAngles.z < 180)
			targetAngle *= -1;
		targetAngle = Mathf.Clamp(targetAngle, -maxAngle, maxAngle);

		// X
		float currentX = recoilTrm.localPosition.x;
		float targetX = Mathf.Clamp(currentX - distance, -maxDistance, originX);

		if (recoilCo != null)
			StopCoroutine(recoilCo);
		recoilCo = StartCoroutine(RecoilCo(currentAngle, targetAngle, currentX, targetX));
	}

	IEnumerator RecoilCo(float startAngle, float targetAngle, float startX, float targetX)
	{
		float timer = 0;
		while (timer < recoilTime)
		{
			timer += Time.deltaTime;
			float per = timer / recoilTime;
			recoilTrm.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(startAngle, targetAngle, per));
			recoilTrm.localPosition = new Vector3(Ease.OutExpo(startX, targetX, per), recoilTrm.localPosition.y, 0);
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
		while (timer < recoveryTime)
		{
			timer += Time.deltaTime;
			float per = timer / recoilTime;
			recoilTrm.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(angle, 0, per));
			recoilTrm.localPosition = new Vector3(Ease.OutExpo(x, originX, per), recoilTrm.localPosition.y, 0);
			yield return null;
		}
		recoilTrm.localRotation = Quaternion.Euler(0, 0, 0);
		recoilCo = null;
	}
}
