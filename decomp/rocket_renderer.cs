using Kobold;
using UnityEngine;

[ExecuteInEditMode]
public class rocket_renderer : BaseMissileWeaponVFX
{
	public GameObject rocketObject;

	public ex3DSprite2 rocketRenderer;

	private exTextureInfo _rocketTexture;

	public string rocketTexture;

	private Vector3 startPos;

	private Vector3 endPos;

	public float dur;

	public float delay;

	public float originalRotation;

	private Color _bodyColor;

	private Color _stripeColor;

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		if (_rocketTexture == null)
		{
			_rocketTexture = SpriteManager.GetTextureInfo(rocketTexture);
		}
		rocketObject.SetActive(value: true);
		base.configure(configuration, path, ref duration);
		rocketRenderer.textureInfo = null;
		rocketRenderer.shader = SpriteManager.GetShaderMode(0);
		if (configuration.paths[path].first != null && configuration.paths[path].last != null)
		{
			startPos = GameManager.Instance.getTileCenter(configuration.paths[path].first.X, configuration.paths[path].first.Y);
			endPos = GameManager.Instance.getTileCenter(configuration.paths[path].last.X, configuration.paths[path].last.Y);
			endPos += new Vector3(Random.Range(-4, 4), Random.Range(-6, 6));
			if (startPos != endPos)
			{
				Vector3 normalized = (endPos - startPos).normalized;
				originalRotation = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f - 90f;
			}
			gameObject.transform.rotation = Quaternion.Euler(0f, 0f, originalRotation);
			float num = 0.02f;
			dur = Vector3.Distance(startPos, endPos) / 24f * num;
			transform.position = startPos;
			duration = dur;
			rocketRenderer.textureInfo = _rocketTexture;
			rocketRenderer.backcolor = new Color(0f, 0f, 0f, 0f);
			if (configuration.paths[path].getConfigValue("bodyColor") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("bodyColor"), out var color))
			{
				_bodyColor = color;
			}
			if (configuration.paths[path].getConfigValue("stripeColor") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("stripeColor"), out var color2))
			{
				_stripeColor = color2;
			}
		}
	}

	public override void pool()
	{
		rocketObject.SetActive(value: false);
		base.pool();
	}

	public override void OnUpdate(float t, float duration)
	{
		if (t <= dur && rocketObject.activeInHierarchy)
		{
			transform.position = endPos;
			rocketRenderer.detailcolor = _bodyColor;
			rocketRenderer.color = _stripeColor;
		}
		else
		{
			rocketObject.SetActive(value: false);
			gameObject.SetActive(value: false);
		}
		if (t > delay)
		{
			if (!rocketObject.activeInHierarchy)
			{
				rocketObject.SetActive(value: true);
			}
			else
			{
				transform.position = Vector3.Lerp(startPos, endPos, (t - delay) / (dur - delay));
			}
		}
	}
}
