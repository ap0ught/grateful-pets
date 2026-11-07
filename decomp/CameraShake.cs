using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public Transform camTransform;

	public static float shakeDuration = 0f;

	public float initialShake = 2f;

	public float shakeFalloff = 0.7f;

	public static float currentShakeAmount = 0.7f;

	public float decreaseFactor = 1f;

	public bool shaking;

	public float TestShakeDuration;

	private Vector3 originalPos;

	private void Awake()
	{
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}
	}

	private void Update()
	{
		if (TestShakeDuration > 0f)
		{
			shakeDuration = TestShakeDuration;
			TestShakeDuration = 0f;
		}
		if (shakeDuration > 0f && !shaking)
		{
			originalPos = camTransform.localPosition;
			shaking = true;
			currentShakeAmount = initialShake;
		}
		if (shaking)
		{
			if (shakeDuration > 0f)
			{
				camTransform.localPosition = originalPos + Random.insideUnitSphere * currentShakeAmount;
				shakeDuration -= Time.deltaTime * decreaseFactor;
				currentShakeAmount -= Time.deltaTime * shakeFalloff;
			}
			else
			{
				shaking = false;
				shakeDuration = 0f;
				camTransform.localPosition = originalPos;
			}
		}
	}
}
