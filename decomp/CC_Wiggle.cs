using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Wiggle")]
public class CC_Wiggle : CC_Base
{
	public float timer;

	public float speed = 1f;

	public float scale = 12f;

	public bool autoTimer = true;

	private void Update()
	{
		if (autoTimer)
		{
			timer += speed * Time.deltaTime;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_Timer", timer);
		material.SetFloat("_Scale", scale);
		Graphics.Blit(source, destination, material);
	}
}
