using UnityEngine;

public class PooledPrefabTemporary : MonoBehaviour
{
	public bool isTemporary;

	public float timeToLive;

	public void Update()
	{
		if (isTemporary)
		{
			timeToLive -= Time.deltaTime;
			if (timeToLive <= 0f)
			{
				PooledPrefabManager.Return(gameObject);
			}
		}
	}
}
