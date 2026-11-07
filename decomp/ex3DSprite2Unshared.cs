using UnityEngine;

[AddComponentMenu("ex2D/3D Sprite2 Unshared")]
public class ex3DSprite2Unshared : ex3DSprite2
{
	public bool ShareMaterial;

	protected override void UpdateMaterial()
	{
		material_ = null;
		cachedRenderer.sharedMaterial = Object.Instantiate(material);
	}
}
