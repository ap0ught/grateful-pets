using UnityEngine;

[ExecuteInEditMode]
public class Spinning : MonoBehaviour
{
	public float speed = 10f;

	private void Start()
	{
	}

	private void Update()
	{
		gameObject.transform.Rotate(0f, 0f, speed * Time.deltaTime * 1f, Space.Self);
	}
}
