using UnityEngine;

[ExecuteInEditMode]
public class MotedAirImposter : MonoBehaviour
{
	private void Awake()
	{
		MotedAirGlobal.wanted++;
	}

	private void LateUpdate()
	{
		MotedAirGlobal.wanted++;
	}
}
