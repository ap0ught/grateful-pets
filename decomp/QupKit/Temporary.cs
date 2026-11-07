using UnityEngine;

namespace QupKit;

public class Temporary : MonoBehaviour
{
	public float Delay;

	public void LateUpdate()
	{
		Delay -= Time.deltaTime;
		if (Delay <= 0f)
		{
			Object.Destroy(gameObject);
		}
	}
}
