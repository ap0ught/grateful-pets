using System;
using ConsoleLib.Console;
using UnityEngine;
using XRL;

[ExecuteInEditMode]
public class IrisdualBeamVFX : BaseMissileWeaponVFX
{
	public LineRenderer Core;

	public LineRenderer Fringe;

	public GameObject Source;

	public GameObject SourceBlaze;

	public ParticleSystem Impact;

	public GameObject ImpactBlaze;

	public Gradient Evaluated;

	public GradientColorKey[] EvaluatedKeys;

	public GradientAlphaKey[] AlphaKeys = new GradientAlphaKey[2]
	{
		new GradientAlphaKey(1f, 0f),
		new GradientAlphaKey(1f, 1f)
	};

	public static Gradient Gradient;

	public static GradientColorKey[] GradientKeys;

	public override void OnUpdate(float t, float duration)
	{
		if (t > duration)
		{
			gameObject.SetActive(value: false);
			return;
		}
		int i = 0;
		for (int num = EvaluatedKeys.Length; i < num; i++)
		{
			EvaluatedKeys[num - 1 - i].color = Gradient.Evaluate(Mathf.Abs(Mathf.Sin((float)i / 7f + t * 2f)));
		}
		Evaluated.SetKeys(EvaluatedKeys, AlphaKeys);
		Fringe.colorGradient = Evaluated;
		Fringe.material.mainTextureOffset = new Vector2((0f - t) * 2f, 0f);
		Core.material.mainTextureOffset = new Vector2((0f - t) * 2f, 0f);
		float num2 = t + duration / 4f;
		int j = 0;
		for (int num3 = EvaluatedKeys.Length; j < num3; j++)
		{
			EvaluatedKeys[num3 - 1 - j].color = Gradient.Evaluate(Mathf.Abs(Mathf.Sin((float)j / 7f + num2 * 2f))) + Color.gray;
		}
		Evaluated.SetKeys(EvaluatedKeys, AlphaKeys);
		Core.colorGradient = Evaluated;
		float num4 = duration * 0.25f;
		if (t >= duration - num4)
		{
			Fringe.startWidth = 20f * (duration - t) / num4;
			Core.startWidth = 10f * (duration - t) / num4;
			ParticleSystem.EmissionModule emission = Impact.emission;
			emission.rateOverTimeMultiplier = 50f * (duration - t) / num4;
		}
	}

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		ConsoleLib.Console.ColorUtility.ColorCollection color = The.Color;
		MissileWeaponVFXConfiguration.MissileVFXPathDefinition missileVFXPathDefinition = configuration.paths[path];
		if (Gradient == null)
		{
			GradientKeys = new GradientColorKey[8]
			{
				new GradientColorKey(color.Red, 0f),
				new GradientColorKey(color.Orange, 0.14f),
				new GradientColorKey(color.Yellow, 0.28f),
				new GradientColorKey(color.Green, 0.43f),
				new GradientColorKey(color.Cyan, 0.57f),
				new GradientColorKey(color.Blue, 0.71f),
				new GradientColorKey(color.Magenta, 0.86f),
				new GradientColorKey(color.Red, 1f)
			};
			Gradient = new Gradient();
			Gradient.SetKeys(GradientKeys, AlphaKeys);
		}
		base.configure(configuration, path, ref duration);
		if (!(missileVFXPathDefinition.first != null) || !(missileVFXPathDefinition.last != null))
		{
			return;
		}
		Vector3 vector = GameManager.Instance.getTileCenter(missileVFXPathDefinition.first.X, missileVFXPathDefinition.first.Y) + new Vector3(0f, 4f);
		Vector3 tileCenter = GameManager.Instance.getTileCenter(missileVFXPathDefinition.last.X, missileVFXPathDefinition.last.Y);
		transform.position = vector;
		Fringe.transform.position = tileCenter;
		Core.transform.position = tileCenter;
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
		Fringe.positionCount = 8;
		Core.positionCount = 8;
		for (int i = 0; i < 8; i++)
		{
			Fringe.SetPosition(i, Vector3.Lerp(vector, tileCenter, (float)i / 7f));
			Core.SetPosition(i, Vector3.Lerp(vector, tileCenter, (float)i / 7f));
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
		if (Evaluated == null)
		{
			Evaluated = new Gradient();
		}
		Evaluated.mode = GradientMode.Blend;
		if (EvaluatedKeys == null)
		{
			EvaluatedKeys = new GradientColorKey[8];
		}
		Array.Copy(GradientKeys, EvaluatedKeys, 8);
		Evaluated.SetKeys(EvaluatedKeys, AlphaKeys);
		Fringe.colorGradient = Evaluated;
		Fringe.startWidth = 20f;
		Fringe.material = Resources.Load<Material>("Prefabs/MissileWeaponsEffects/BeamFringeMat");
		Core.colorGradient = Evaluated;
		Core.startWidth = 10f;
		Core.material = Resources.Load<Material>("Prefabs/MissileWeaponsEffects/BeamCoreMat");
	}
}
