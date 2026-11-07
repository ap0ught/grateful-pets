using System;
using Kobold;
using UnityEngine;

[ExecuteInEditMode]
public class arrow_renderer : BaseMissileWeaponVFX
{
	public GameObject arrowObject;

	public ex3DSprite2 arrowRenderer;

	public TrailRenderer trailRenderer;

	private exTextureInfo _arrowTexture;

	public string arrowTexture;

	private exTextureInfo _impactTexture;

	public string impactTexture;

	private Vector3 startPos;

	private Vector3 endPos;

	public float dur;

	public float stickDur;

	public float delay;

	public float originalRotation;

	public float wobbleAmplitude = 10f;

	public float wobbleFrequency = 0.33f;

	private Color _headColor;

	private Color _shaftColor;

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		if (_arrowTexture == null)
		{
			_arrowTexture = SpriteManager.GetTextureInfo(arrowTexture);
		}
		if (_impactTexture == null)
		{
			_impactTexture = SpriteManager.GetTextureInfo(impactTexture);
		}
		arrowObject.SetActive(value: true);
		base.configure(configuration, path, ref duration);
		trailRenderer.Clear();
		arrowRenderer.textureInfo = null;
		arrowRenderer.shader = SpriteManager.GetShaderMode(0);
		if (configuration.paths[path].first != null && configuration.paths[path].last != null)
		{
			startPos = GameManager.Instance.getTileCenter(configuration.paths[path].first.X, configuration.paths[path].first.Y);
			endPos = GameManager.Instance.getTileCenter(configuration.paths[path].last.X, configuration.paths[path].last.Y);
			endPos += new Vector3(UnityEngine.Random.Range(-4, 4), UnityEngine.Random.Range(-6, 6));
			if (startPos != endPos)
			{
				Vector3 normalized = (endPos - startPos).normalized;
				originalRotation = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f - 90f;
			}
			gameObject.transform.rotation = Quaternion.Euler(0f, 0f, originalRotation);
			float num = 0.02f;
			dur = Vector3.Distance(startPos, endPos) / 24f * num;
			delay = (float)path * 0.02f;
			dur += delay;
			transform.position = startPos;
			trailRenderer.Clear();
			if (dur + stickDur > duration)
			{
				duration = dur + stickDur;
			}
			arrowRenderer.textureInfo = _arrowTexture;
			arrowRenderer.backcolor = new Color(0f, 0f, 0f, 0f);
			if (configuration.paths[path].getConfigValue("shaftColor") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("shaftColor"), out var color))
			{
				_shaftColor = color;
			}
			if (configuration.paths[path].getConfigValue("headColor") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("headColor"), out var color2))
			{
				_headColor = color2;
			}
			if (configuration.paths[path].getConfigValue("trailColor") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("trailColor"), out var color3))
			{
				trailRenderer.startColor = color3;
			}
		}
	}

	public override void pool()
	{
		trailRenderer.Clear();
		arrowObject.SetActive(value: false);
		base.pool();
	}

	public override void OnUpdate(float t, float duration)
	{
		if (t <= dur && arrowObject.activeInHierarchy)
		{
			transform.position = endPos;
			arrowRenderer.detailcolor = _shaftColor;
			arrowRenderer.color = _headColor;
		}
		else if (t <= dur + stickDur && arrowObject.activeInHierarchy)
		{
			transform.position = endPos;
			float f = (t - dur) % wobbleFrequency / wobbleFrequency * (MathF.PI * 2f);
			float t2 = (t - dur) / stickDur;
			transform.rotation = Quaternion.Euler(0f, 0f, originalRotation + Mathf.Lerp(wobbleAmplitude * Mathf.Sin(f), 0f, t2));
			if (arrowRenderer.textureInfo != _impactTexture)
			{
				arrowRenderer.textureInfo = _impactTexture;
				arrowRenderer.detailcolor = _shaftColor;
				arrowRenderer.color = _headColor;
			}
			trailRenderer.Clear();
		}
		else if (t > dur + stickDur)
		{
			trailRenderer.Clear();
			arrowObject.SetActive(value: false);
			gameObject.SetActive(value: false);
		}
		if (t > delay)
		{
			if (!arrowObject.activeInHierarchy)
			{
				arrowObject.SetActive(value: true);
			}
			else
			{
				transform.position = Vector3.Lerp(startPos, endPos, (t - delay) / (dur - delay));
			}
		}
	}
}
