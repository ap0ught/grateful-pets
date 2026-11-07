using UnityEngine;

public class OrthoCamera : MonoBehaviour
{
	protected void Awake()
	{
		gameObject.GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;
	}
}
