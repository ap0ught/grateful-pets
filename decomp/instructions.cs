using UnityEngine;

public class instructions : MonoBehaviour
{
	public void OnBackButtonPressed()
	{
		Object.Instantiate(Resources.Load("Main Menu"));
		Object.DestroyImmediate(gameObject);
	}
}
