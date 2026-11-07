using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;

[ExecuteInEditMode]
public class ProjectileRenderer : BaseMissileWeaponVFX
{
	public GameObject Object;

	public ex3DSprite2 Projectile;

	public TrailRenderer Trail;

	public ParticleSystem Particles;

	public Vector3 TrailOrigin;

	public Color Foreground;

	public Color Detail;

	public float Speed;

	public float Orientation;

	public float Rotation;

	public float Arc;

	public float Curve;

	public int Slant;

	public float Rate;

	private Vector3 Start;

	private Vector3 End;

	private Vector3 Scale = Vector3.one;

	private float StartRotation;

	private float EndRotation;

	private float Delay;

	private Easing.Functions Interpolation;

	public float ParticleDuration;

	public void LoadFields(Dictionary<string, string> Values)
	{
		if (TryGetInheritable(Values, "Tile", "RenderTile", out var Value))
		{
			Projectile.textureInfo = SpriteManager.GetTextureInfo(Value);
			Projectile.shader = SpriteManager.GetShaderMode(Projectile.textureInfo.ShaderMode);
		}
		if (TryGetInheritable(Values, "Foreground", "RenderForeground", out Value))
		{
			Projectile.color = (Foreground = ParseColor(Value));
		}
		if (TryGetInheritable(Values, "Detail", "RenderDetail", out Value))
		{
			Projectile.detailcolor = (Detail = ParseColor(Value));
		}
		if (Values.TryGetValue("Orientation", out Value))
		{
			Orientation = ParseSingle(Value);
		}
		if (Values.TryGetValue("Rotation", out Value))
		{
			Rotation = ParseSingle(Value);
		}
		if (Values.TryGetValue("Scale", out Value))
		{
			Scale = Vector3.one * ParseSingle(Value);
		}
		if (TryGetInheritable(Values, "Flip", "RenderFlip", out Value))
		{
			switch (char.ToUpperInvariant(Value[0]))
			{
			case 'H':
				Scale = new Vector3(Scale.x * -1f, Scale.y, Scale.z);
				break;
			case 'V':
				Scale = new Vector3(Scale.x, Scale.y * -1f, Scale.z);
				break;
			case 'B':
				Scale = new Vector3(Scale.x * -1f, Scale.y * -1f, Scale.z);
				break;
			}
		}
		if (Values.TryGetValue("Speed", out Value))
		{
			Speed = ParseSingle(Value);
		}
		if (Values.TryGetValue("Rate", out Value))
		{
			Rate = ParseSingle(Value);
		}
		if (Values.TryGetValue("Arc", out Value))
		{
			Arc = ParseSingle(Value);
		}
		if (Values.TryGetValue("Slant", out Value))
		{
			switch (Value.ToLowerInvariant())
			{
			case "false":
			case "0":
			case "":
				Slant = 0;
				break;
			case "true":
			case "1":
				Slant = 1;
				break;
			case "absolute":
				Slant = 2;
				break;
			case "absolutex":
				Slant = 3;
				break;
			case "absolutey":
				Slant = 4;
				break;
			}
		}
		if (Values.TryGetValue("Curve", out Value))
		{
			Curve = ParseSingle(Value);
		}
		if (Values.TryGetValue("Easing", out Value))
		{
			Interpolation = Value switch
			{
				"SineIn" => Easing.Functions.SineEaseIn, 
				"SineOut" => Easing.Functions.SineEaseOut, 
				"SineInOut" => Easing.Functions.SineEaseInOut, 
				_ => Easing.Functions.SineEaseInOut, 
			};
		}
	}

	public void LoadTrail(Dictionary<string, string> Values)
	{
		if (TryGetInheritable(Values, "TrailColor", "RenderForeground", out var Value))
		{
			Trail.startColor = ParseColor(Value);
			Trail.endColor = Trail.startColor.WithAlpha(0f);
		}
		if (TryGetInheritable(Values, "TrailStartColor", "RenderForeground", out Value))
		{
			Trail.startColor = ParseColor(Value);
		}
		if (TryGetInheritable(Values, "TrailEndColor", "RenderDetail", out Value))
		{
			Trail.endColor = ParseColor(Value);
			if (!Value.IsNullOrEmpty() && (Value[0] != '#' || Value.Length != 9))
			{
				Trail.endColor = Trail.endColor.WithAlpha(0f);
			}
		}
		if (Values.TryGetValue("TrailWidth", out Value))
		{
			Trail.startWidth = ParseSingle(Value);
			Trail.endWidth = Trail.startWidth;
		}
		if (Values.TryGetValue("TrailStartWidth", out Value))
		{
			Trail.startWidth = ParseSingle(Value);
		}
		if (Values.TryGetValue("TrailEndWidth", out Value))
		{
			Trail.endWidth = ParseSingle(Value);
		}
		if (Values.TryGetValue("TrailOrigin", out Value))
		{
			Value.AsDelimitedSpans(',', out var First, out var Second);
			if (float.TryParse(First, out var result) && float.TryParse(Second, out var result2))
			{
				Trail.transform.localPosition = (TrailOrigin = new Vector3(result, result2, 10f));
			}
		}
		if (Values.TryGetValue("TrailLife", out Value))
		{
			Trail.time = ParseSingle(Value);
			if (Trail.time <= 0f)
			{
				Trail.enabled = false;
			}
		}
		if (Values.TryGetValue("TrailOrientation", out Value))
		{
			Trail.transform.localRotation = Quaternion.Euler(0f, 0f, ParseSingle(Value));
		}
	}

	public void LoadParticles(Dictionary<string, string> Values)
	{
		ParticleSystem.MainModule main = Particles.main;
		ParticleSystem.ShapeModule shape = Particles.shape;
		ParticleSystem.ColorOverLifetimeModule colorOverLifetime = Particles.colorOverLifetime;
		ParticleSystem.EmissionModule emission = Particles.emission;
		if (Values.TryGetValue("ParticleShape", out var value) && value == "Rectangle")
		{
			shape.shapeType = ParticleSystemShapeType.Rectangle;
		}
		if (Values.TryGetValue("ParticleShapePosition", out value))
		{
			shape.position = ParseVector(value);
		}
		if (Values.TryGetValue("ParticleShapeRotation", out value))
		{
			shape.rotation = ParseVector(value);
		}
		if (Values.TryGetValue("ParticleShapeScale", out value))
		{
			shape.scale = ParseVector(value);
		}
		if (Values.TryGetValue("ParticleLife", out value))
		{
			float num = ParseSingle(value);
			if (num > 0f)
			{
				main.startLifetime = new ParticleSystem.MinMaxCurve(num);
			}
		}
		if (Values.TryGetValue("ParticleRate", out value))
		{
			float num2 = ParseSingle(value);
			if (num2 > 0f)
			{
				emission.rateOverTime = new ParticleSystem.MinMaxCurve(num2);
				ParticleDuration = 1f;
			}
		}
		if (Values.TryGetValue("ParticleGravity", out value))
		{
			main.gravityModifier = new ParticleSystem.MinMaxCurve(ParseSingle(value));
		}
		if (Values.TryGetValue("ParticleSpherize", out value))
		{
			shape.sphericalDirectionAmount = ParseSingle(value);
		}
		if (TryGetInheritable(Values, "ParticleColor", "RenderForeground", out value))
		{
			Color color = ParseColor(value);
			colorOverLifetime.color = new ParticleSystem.MinMaxGradient(color, color.WithAlpha(1f));
		}
		if (TryGetInheritable(Values, "ParticleStartColor", "RenderForeground", out value))
		{
			Color color2 = ParseColor(value);
			colorOverLifetime.color = new ParticleSystem.MinMaxGradient(color2, color2.WithAlpha(1f));
		}
		if (TryGetInheritable(Values, "ParticleEndColor", "RenderDetail", out value))
		{
			Color color3 = ParseColor(value);
			if (!value.IsNullOrEmpty() && (value[0] != '#' || value.Length != 9))
			{
				color3 = color3.WithAlpha(1f);
			}
			colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colorOverLifetime.color.colorMin, color3);
		}
	}

	public override void configure(MissileWeaponVFXConfiguration configuration, int path, ref float duration)
	{
		base.configure(configuration, path, ref duration);
		Object.SetActive(value: false);
		Reset();
		MissileWeaponVFXConfiguration.MissileVFXPathDefinition missileVFXPathDefinition = configuration.paths[path];
		float num = 1f;
		if (missileVFXPathDefinition.first != null && missileVFXPathDefinition.last != null)
		{
			Start = GameManager.Instance.getTileCenter(missileVFXPathDefinition.first.X, missileVFXPathDefinition.first.Y);
			End = GameManager.Instance.getTileCenter(missileVFXPathDefinition.last.X, missileVFXPathDefinition.last.Y);
		}
		LoadFields(missileVFXPathDefinition.projectileVFXConfiguration);
		LoadTrail(missileVFXPathDefinition.projectileVFXConfiguration);
		LoadParticles(missileVFXPathDefinition.projectileVFXConfiguration);
		num = (duration = Vector3.Distance(Start, End) / Speed);
		Transform transform = base.transform;
		Projectile.transform.localPosition = new Vector3(0f, 0f, -10f);
		Delay = (float)path * (Rate / 1000f);
		duration += Delay;
		transform.position = Start;
		float num2 = Orientation;
		if (Start != End)
		{
			Vector3 normalized = (End - Start).normalized;
			num2 += Mathf.Atan2(normalized.y, normalized.x) * 57.29578f - 90f;
		}
		StartRotation = num2;
		EndRotation = num2 + Rotation * num;
		transform.localRotation = Quaternion.Euler(0f, 0f, num2);
		transform.localScale = Scale;
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
				if (Value == "Foreground")
				{
					return Foreground;
				}
				if (Value == "Detail")
				{
					return Detail;
				}
				ConsoleLib.Console.ColorUtility.ColorMap.TryGetValue(Value[0], out value);
			}
		}
		return value;
	}

	public ParticleSystem.MinMaxCurve ParseCurve(ReadOnlySpan<char> Value)
	{
		return default;
	}

	public Vector3 ParseVector(ReadOnlySpan<char> Value, char SplitVector = ',', char SplitSingle = ';')
	{
		Vector3 result = default;
		if (Value.Length != 0)
		{
			Value.Split(',', out var First, out var Second, out var Third);
			result = new Vector3(ParseSingle(First, SplitSingle), ParseSingle(Second, SplitSingle), ParseSingle(Third, SplitSingle));
		}
		return result;
	}

	public float ParseSingle(ReadOnlySpan<char> Value, char Split = ',')
	{
		float result = 0f;
		if (Value.Length != 0)
		{
			Value.Split(Split, out var First, out var Second);
			float result2;
			float result3;
			if (Second.Length == 0)
			{
				if (First[0] == '?')
				{
					float.TryParse(First.Slice(1), out result);
					if (UnityEngine.Random.value >= 0.5f)
					{
						result *= -1f;
					}
				}
				else
				{
					float.TryParse(First, out result);
				}
			}
			else if (float.TryParse(First, out result2) && float.TryParse(Second, out result3))
			{
				result = UnityEngine.Random.Range(result2, result3);
			}
		}
		return result;
	}

	public bool TryGetInheritable(Dictionary<string, string> Values, string Key, string InheritKey, out string Value)
	{
		if (Values.TryGetValue(Key, out Value))
		{
			if (!(Value != "Inherit"))
			{
				return Values.TryGetValue(InheritKey, out Value);
			}
			return true;
		}
		return false;
	}

	public void Reset()
	{
		Trail.Clear();
		Foreground = default;
		Detail = default;
		Speed = 0f;
		Orientation = 0f;
		StartRotation = 0f;
		EndRotation = 0f;
		Arc = 0f;
		Slant = 0;
		Curve = 0f;
		Interpolation = Easing.Functions.Linear;
		Trail.enabled = true;
		Trail.startColor = Color.white;
		Trail.endColor = Color.white.WithAlpha(0f);
		TrailRenderer trail = Trail;
		float startWidth = (Trail.endWidth = 1f);
		trail.startWidth = startWidth;
		Trail.transform.localPosition = default;
		Trail.time = 0.1f;
		Particles.gameObject.SetActive(value: false);
		ParticleDuration = 0f;
		ParticleSystem.MainModule main = Particles.main;
		ParticleSystem.ShapeModule shape = Particles.shape;
		ParticleSystem.ColorOverLifetimeModule colorOverLifetime = Particles.colorOverLifetime;
		ParticleSystem.EmissionModule emission = Particles.emission;
		main.gravityModifier = 0f;
		emission.rateOverTime = 0f;
		colorOverLifetime.color = new ParticleSystem.MinMaxGradient(Color.white, Color.white.WithAlpha(0f));
		shape.shapeType = ParticleSystemShapeType.Rectangle;
		shape.position = Vector3.zero;
		shape.rotation = Vector3.zero;
		shape.scale = new Vector3(16f, 24f, 1f);
		shape.randomDirectionAmount = 0f;
		shape.sphericalDirectionAmount = 0.1f;
		shape.randomPositionAmount = 0f;
		transform.localScale = (Scale = Vector3.one);
	}

	public override void pool()
	{
		Object.SetActive(value: false);
		Reset();
		base.pool();
	}

	public override void start()
	{
		base.start();
		if (ParticleDuration > 0f)
		{
			ParticleSystem.EmissionModule emission = Particles.emission;
			Particles.gameObject.SetActive(value: true);
			if (!Particles.isStopped)
			{
				Particles.Stop();
			}
			Particles.Play();
			emission.enabled = true;
		}
	}

	public override void OnUpdate(float t, float duration)
	{
		Projectile.color = Foreground;
		Projectile.detailcolor = Detail;
		Projectile.backcolor = Color.clear;
		if (t >= duration && Object.activeInHierarchy)
		{
			transform.position = End;
		}
		else if (t > duration)
		{
			Trail.Clear();
			Particles.gameObject.SetActive(value: false);
			Object.SetActive(value: false);
			gameObject.SetActive(value: false);
		}
		if (!(t > Delay))
		{
			return;
		}
		if (!Object.activeInHierarchy)
		{
			Object.SetActive(value: true);
			return;
		}
		float num = (t - Delay) / (duration - Delay);
		float num2 = Mathf.Sin(num * MathF.PI);
		Vector3 position = Vector3.Lerp(Start, End, Easing.Interpolate(num, Interpolation));
		if (Slant == 0)
		{
			position += new Vector3(Curve * num2, Arc * num2, 0f);
		}
		else
		{
			float num3 = Start.y - End.y;
			float num4 = Start.x - End.x;
			int slant = Slant;
			if (slant == 2 || slant == 3)
			{
				num3 = Math.Abs(num3);
			}
			slant = Slant;
			if (slant == 2 || slant == 4)
			{
				num4 = Math.Abs(num4);
			}
			position += new Vector3(num3 / 24f * Curve * num2, num4 / 16f * Arc * num2, 0f);
		}
		transform.position = position;
		transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(StartRotation, EndRotation, num));
	}
}
