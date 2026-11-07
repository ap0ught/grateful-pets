using UnityEngine;

public class PooledPrefab : MonoBehaviour
{
	public GameObject originalPrefab;

	public void Return()
	{
		PooledPrefabManager.Return(gameObject);
	}
}
