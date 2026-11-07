using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Posterize")]
public class CC_Posterize : CC_Base
{
	[Range(2f, 255f)]
	public int levels = 4;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_Levels", levels);
		Graphics.Blit(source, destination, material);
	}
}
