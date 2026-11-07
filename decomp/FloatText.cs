using UnityEngine;

public class FloatText : MonoBehaviour
{
	private float Speed = 35f;

	public double Duration = 2.0;

	private double StartTime;

	private void Start()
	{
		StartTime = Time.fixedTime;
	}

	private void Update()
	{
		if ((double)Time.fixedTime - StartTime > Duration)
		{
			Object.Destroy(gameObject);
		}
		else
		{
			gameObject.transform.position += new Vector3(0f, Speed * Time.deltaTime, 0f);
		}
	}
}
