using System;
using UnityEngine;

public class Temporary : MonoBehaviour
{
	[NonSerialized]
	private bool Destroyed;

	public float Duration = 1025f;

	public Vector3 Delta = new Vector3(0f, 0f, 0f);

	public Action BeforeDestroy;

	public Temporary()
	{
	}

	public Temporary(float _Duration)
	{
		Duration = _Duration * 1000f;
	}

	private void Start()
	{
	}

	private void Update()
	{
		Duration -= Time.deltaTime;
		if (Duration < 0f && !Destroyed)
		{
			Destroyed = true;
			if (BeforeDestroy != null)
			{
				BeforeDestroy();
			}
			UnityEngine.Object.Destroy(gameObject);
		}
		else
		{
			transform.position += Delta * Time.deltaTime;
		}
	}
}
