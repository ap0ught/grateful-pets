using UnityEngine;

[ExecuteAlways]
public class VLS_Missle_VFX : MonoBehaviour
{
	public float speed;

	private void Start()
	{
		transform.localPosition = new Vector3(0f, 0f, 0f);
		speed = 0f;
	}

	private void Update()
	{
		transform.localPosition += new Vector3(0f, (500f + speed) * Time.deltaTime, 0f);
		speed += 500f * Time.deltaTime;
	}
}
