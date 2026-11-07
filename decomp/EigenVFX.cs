using System;
using ConsoleLib.Console;
using UnityEngine;

[ExecuteInEditMode]
public class EigenVFX : BaseMissileWeaponVFX
{
	public LineRenderer Left;

	public LineRenderer Right;

	public GameObject Source;

	public GameObject SourceBlaze;

	public ParticleSystem Impact;

	public GameObject ImpactBlaze;

	public float Tessellation;

	public float Width = 8f;

	public override void OnUpdate(float t, float duration)
	{
		if (t > duration)
		{
			gameObject.SetActive(value: false);
			return;
		}
		float num = duration * 0.9f;
		if (t >= duration - num)
		{
			Right.startWidth = 20f * (duration - t) / num;
			Left.startWidth = 10f * (duration - t) / num;
			ParticleSystem.EmissionModule emission = Impact.emission;
			emission.rateOverTimeMultiplier = 50f * (duration - t) / num;
		}
	}

	public Color ParseColor(string Value)
	{
		Color value = default;
		if (!Value.IsNullOrEmpty())
		{
			if (Value[0] == '#')
			{
				UnityEngine.ColorUtility.TryParseHtmlString(Value, out value);
			}
			else
			{
				ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(Value[0], out value);
			}
		}
		return value;
	}

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		MissileWeaponVFXConfiguration.MissileVFXPathDefinition missileVFXPathDefinition = configuration.paths[path];
		base.configure(configuration, path, ref duration);
		if (!(missileVFXPathDefinition.first != null) || !(missileVFXPathDefinition.last != null))
		{
			return;
		}
		duration = 0.25f;
		Vector3 vector = GameManager.Instance.getTileCenter(missileVFXPathDefinition.first.X, missileVFXPathDefinition.first.Y) + new Vector3(0f, 4f);
		Vector3 tileCenter = GameManager.Instance.getTileCenter(missileVFXPathDefinition.last.X, missileVFXPathDefinition.last.Y);
		transform.position = vector;
		Right.transform.position = tileCenter;
		Left.transform.position = tileCenter;
		if ((bool)Source)
		{
			Source.transform.position = vector;
		}
		if ((bool)Impact)
		{
			Impact.transform.position = tileCenter;
			ParticleSystem.EmissionModule emission = Impact.emission;
			emission.rateOverTimeMultiplier = 300f;
		}
		if (missileVFXPathDefinition.projectileVFXConfiguration.TryGetValue("Color", out var value))
		{
			LineRenderer right = Right;
			LineRenderer right2 = Right;
			LineRenderer left = Left;
			Color color = (Left.startColor = ParseColor(value));
			Color color3 = (left.endColor = color);
			Color endColor = (right2.startColor = color3);
			right.endColor = endColor;
		}
		if (missileVFXPathDefinition.projectileVFXConfiguration.TryGetValue("LeftColor", out value))
		{
			LineRenderer left2 = Left;
			Color endColor = (Left.startColor = ParseColor(value));
			left2.endColor = endColor;
		}
		if (missileVFXPathDefinition.projectileVFXConfiguration.TryGetValue("RightColor", out value))
		{
			LineRenderer right3 = Right;
			Color endColor = (Right.startColor = ParseColor(value));
			right3.endColor = endColor;
		}
		if (missileVFXPathDefinition.projectileVFXConfiguration.TryGetValue("StartColor", out value))
		{
			LineRenderer right4 = Right;
			Color endColor = (Left.startColor = ParseColor(value));
			right4.startColor = endColor;
		}
		if (missileVFXPathDefinition.projectileVFXConfiguration.TryGetValue("EndColor", out value))
		{
			LineRenderer right5 = Right;
			Color endColor = (Left.endColor = ParseColor(value));
			right5.endColor = endColor;
		}
		if (missileVFXPathDefinition.projectileVFXConfiguration.TryGetValue("Tessellation", out value))
		{
			Tessellation = float.Parse(value);
		}
		if (missileVFXPathDefinition.projectileVFXConfiguration.TryGetValue("Width", out value))
		{
			Width = float.Parse(value);
		}
		float num = Vector3.Distance(vector, tileCenter) / 1280f;
		int num2 = Math.Max(3, Mathf.RoundToInt(Tessellation * num));
		Right.positionCount = num2;
		Left.positionCount = num2;
		Vector3 normalized = (tileCenter - vector).normalized;
		Quaternion quaternion = Quaternion.Euler(0f, 0f, Mathf.Atan2(normalized.y, normalized.x) * 57.29578f - 90f);
		int i = 0;
		for (int num3 = num2 - 1; i <= num3; i++)
		{
			Vector3 vector2 = Vector3.Lerp(vector, tileCenter, (float)i / ((float)num2 - 1f));
			Vector3 vector3 = Vector3.zero;
			if (i > 0 && i < num3)
			{
				vector3 = ((i % 2 != 0) ? (quaternion * new Vector3(8f, 0f, 0f)) : (quaternion * new Vector3(-8f, 0f, 0f)));
			}
			Right.SetPosition(i, vector2 + vector3);
			Left.SetPosition(i, vector2 - vector3);
		}
		if (path != 0)
		{
			if ((bool)Source)
			{
				Source.SetActive(value: false);
			}
			if ((bool)SourceBlaze)
			{
				SourceBlaze.SetActive(value: false);
			}
		}
		else
		{
			if ((bool)Source)
			{
				Source.SetActive(value: true);
			}
			if ((bool)SourceBlaze)
			{
				SourceBlaze.SetActive(value: true);
			}
		}
		Right.startWidth = 3f;
		Right.material = Resources.Load<Material>("Prefabs/MissileWeaponsEffects/BeamCoreMat");
		Left.startWidth = 3f;
		Left.material = Resources.Load<Material>("Prefabs/MissileWeaponsEffects/BeamCoreMat");
	}
}
