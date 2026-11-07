using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Pixelate")]
public class CC_Pixelate : CC_Base
{
	[Range(1f, 1024f)]
	public float scale = 80f;

	public bool automaticRatio;

	public float ratio = 1f;

	public int mode;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		switch (mode)
		{
		case 0:
			material.SetFloat("_Scale", scale);
			break;
		default:
			material.SetFloat("_Scale", (float)GetComponent<Camera>().pixelWidth / scale);
			break;
		}
		material.SetFloat("_Ratio", automaticRatio ? ((float)(GetComponent<Camera>().pixelWidth / GetComponent<Camera>().pixelHeight)) : ratio);
		Graphics.Blit(source, destination, material);
	}
}
