using System;
using Kobold;
using UnityEngine;

[ExecuteInEditMode]
public class vls_impact_renderer : MonoBehaviour, CombatJuice.ICombatJuiceAnimator
{
	public GameObject missileVfx;

	public ex3DSprite2 rocketRenderer;

	private exTextureInfo _rocketTexture;

	public string rocketTexture = "Assets_Content_Textures_Combat3C_rocket.png";

	public float t;

	public float duration = 1f;

	public void Update()
	{
		t += Time.deltaTime;
		if (t > duration)
		{
			t = duration;
		}
		missileVfx.transform.localPosition = new Vector3(0f, Mathf.Lerp(900f, 0f, t / duration), 0f);
		if (t >= duration)
		{
			gameObject.SetActive(value: false);
		}
	}

	public void Play(bool loop = false, Action after = null, string name = null, string objectId = null)
	{
		duration = 0.5f;
		t = 0f;
		missileVfx.transform.localPosition = new Vector3(0f, 900f, 0f);
		if (_rocketTexture == null)
		{
			_rocketTexture = SpriteManager.GetTextureInfo(rocketTexture);
		}
		rocketRenderer.textureInfo = null;
		rocketRenderer.shader = SpriteManager.GetShaderMode(0);
		rocketRenderer.textureInfo = _rocketTexture;
		rocketRenderer.backcolor = new Color(0f, 0f, 0f, 0f);
		rocketRenderer.color = new Color(0f, 1f, 1f, 1f);
		rocketRenderer.detailcolor = new Color(1f, 0f, 0f, 1f);
	}

	public void Stop()
	{
	}
}
