using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Cross Stitch")]
public class CC_CrossStitch : CC_Base
{
	[Range(1f, 128f)]
	public int size = 8;

	public float brightness = 1.5f;

	public bool invert;

	public bool pixelize = true;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_StitchSize", size);
		material.SetFloat("_Brightness", brightness);
		int num = (invert ? 1 : 0);
		if (pixelize)
		{
			num += 2;
			material.SetFloat("_Scale", GetComponent<Camera>().pixelWidth / size);
			material.SetFloat("_Ratio", GetComponent<Camera>().pixelWidth / GetComponent<Camera>().pixelHeight);
		}
		Graphics.Blit(source, destination, material, num);
	}
}
