using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Vibrance")]
public class CC_Vibrance : CC_Base
{
	[Range(-100f, 100f)]
	public float amount;

	[Range(-5f, 5f)]
	public float redChannel = 1f;

	[Range(-5f, 5f)]
	public float greenChannel = 1f;

	[Range(-5f, 5f)]
	public float blueChannel = 1f;

	public bool advanced;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (amount == 0f)
		{
			Graphics.Blit(source, destination);
		}
		else if (advanced)
		{
			material.SetFloat("_Amount", amount * 0.01f);
			material.SetVector("_Channels", new Vector3(redChannel, greenChannel, blueChannel));
			Graphics.Blit(source, destination, material, 1);
		}
		else
		{
			material.SetFloat("_Amount", amount * 0.02f);
			Graphics.Blit(source, destination, material, 0);
		}
	}
}
