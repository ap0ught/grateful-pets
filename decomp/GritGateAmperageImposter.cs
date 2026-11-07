using UnityEngine;
using XRL.UI;

[ExecuteInEditMode]
public class GritGateAmperageImposter : MonoBehaviour
{
	public static string display = "Amps Reminaing: <updating...>";

	private TextMesh mesh;

	private void Start()
	{
	}

	private void Update()
	{
		if (mesh == null)
		{
			mesh = GetComponent<TextMesh>();
		}
		if (mesh != null && mesh.text != display)
		{
			mesh.text = Sidebar.FormatToRTF(display);
		}
	}
}
