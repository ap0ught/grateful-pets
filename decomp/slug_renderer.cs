using UnityEngine;

[ExecuteInEditMode]
public class slug_renderer : BaseMissileWeaponVFX
{
	public GameObject slugObject;

	public SpriteRenderer slugRenderer;

	public TrailRenderer trailRenderer;

	private Vector3 startPos;

	private Vector3 endPos;

	public float dur;

	public float delay;

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		base.configure(configuration, path, ref duration);
		slugObject.SetActive(value: false);
		trailRenderer.Clear();
		if (configuration.paths[path].first != null && configuration.paths[path].last != null)
		{
			startPos = GameManager.Instance.getTileCenter(configuration.paths[path].first.X, configuration.paths[path].first.Y);
			endPos = GameManager.Instance.getTileCenter(configuration.paths[path].last.X, configuration.paths[path].last.Y);
			float num = 0.02f;
			dur = Vector3.Distance(startPos, endPos) / 24f * num;
			delay = (float)path * 0.02f;
			dur += delay;
			transform.position = startPos;
			trailRenderer.Clear();
			if (dur > duration)
			{
				duration = dur;
			}
			if (configuration.paths[path].getConfigValue("slugColor") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("slugColor"), out var color))
			{
				slugRenderer.color = color;
			}
			if (configuration.paths[path].getConfigValue("trailColor") != null && ColorUtility.TryParseHtmlString(configuration.paths[path].getConfigValue("trailColor"), out var color2))
			{
				trailRenderer.startColor = color2;
			}
		}
	}

	public override void pool()
	{
		trailRenderer.Clear();
		slugObject.SetActive(value: false);
		base.pool();
	}

	public override void OnUpdate(float t, float duration)
	{
		if (t >= dur && slugObject.activeInHierarchy)
		{
			transform.position = endPos;
		}
		else if (t > dur)
		{
			trailRenderer.Clear();
			slugObject.SetActive(value: false);
			gameObject.SetActive(value: false);
		}
		if (t > delay)
		{
			if (!slugObject.activeInHierarchy)
			{
				slugObject.SetActive(value: true);
			}
			else
			{
				transform.position = Vector3.Lerp(startPos, endPos, (t - delay) / (dur - delay));
			}
		}
	}
}
