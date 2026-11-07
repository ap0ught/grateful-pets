using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Blend")]
public class CC_Blend : CC_Base
{
	public Texture texture;

	[Range(0f, 1f)]
	public float amount = 1f;

	public int mode;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (texture == null || amount == 0f)
		{
			Graphics.Blit(source, destination);
			return;
		}
		material.SetTexture("_OverlayTex", texture);
		material.SetFloat("_Amount", amount);
		Graphics.Blit(source, destination, material, mode);
	}
}
