using UnityEngine;

[ExecuteInEditMode]
public class blaster_renderer : BaseMissileWeaponVFX
{
	public ParticleSystem trails;

	private Vector3 startPos;

	private Vector3 endPos;

	public float dur;

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		base.configure(configuration, path, ref duration);
		if (configuration.paths[path].first != null && configuration.paths[path].last != null)
		{
			startPos = GameManager.Instance.getTileCenter(configuration.paths[path].first.X, configuration.paths[path].first.Y);
			endPos = GameManager.Instance.getTileCenter(configuration.paths[path].last.X, configuration.paths[path].last.Y);
			float num = 0.05f;
			dur = Vector3.Distance(startPos, endPos) / 24f * num;
			if (dur > duration)
			{
				duration = dur;
			}
			trails.Stop();
			transform.position = startPos;
			trails.Play();
		}
	}

	public override void OnUpdate(float t, float duration)
	{
		if (t > dur)
		{
			gameObject.SetActive(value: false);
		}
		Debug.Log($"{startPos} {endPos} {t} {dur}");
		transform.position = Vector3.Lerp(startPos, endPos, t / dur);
	}
}
