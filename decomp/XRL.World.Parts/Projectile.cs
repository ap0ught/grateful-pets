using System;

namespace XRL.World.Parts;

[Serializable]
public class Projectile : IPart
{
	public int BasePenetration = 1;

	public int StrengthPenetration;

	public bool PenetrateCreatures;

	public bool PenetrateWalls;

	public bool Quiet;

	public string BaseDamage = "1d4";

	public string ColorString;

	public string Attributes = "";

	public string PassByVerb = "whiz";

	public string RenderChar;

	[NonSerialized]
	public GameObject Launcher;

	public override bool SameAs(IPart p)
	{
		Projectile projectile = p as Projectile;
		if (projectile.BasePenetration != BasePenetration)
		{
			return false;
		}
		if (projectile.StrengthPenetration != StrengthPenetration)
		{
			return false;
		}
		if (projectile.PenetrateCreatures != PenetrateCreatures)
		{
			return false;
		}
		if (projectile.PenetrateWalls != PenetrateWalls)
		{
			return false;
		}
		if (projectile.Quiet != Quiet)
		{
			return false;
		}
		if (projectile.BaseDamage != BaseDamage)
		{
			return false;
		}
		if (projectile.ColorString != ColorString)
		{
			return false;
		}
		if (projectile.Attributes != Attributes)
		{
			return false;
		}
		if (projectile.PassByVerb != PassByVerb)
		{
			return false;
		}
		if (projectile.Launcher != Launcher)
		{
			return false;
		}
		if (projectile.RenderChar != RenderChar)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == PooledEvent<GetPowerLoadLevelEvent>.ID)
			{
				return Launcher != null;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetPowerLoadLevelEvent E)
	{
		if (GameObject.Validate(ref Launcher))
		{
			int powerLoadLevel = Launcher.GetPowerLoadLevel();
			if (powerLoadLevel > E.Level)
			{
				E.Level = powerLoadLevel;
			}
		}
		return base.HandleEvent(E);
	}

	public bool HasAttribute(string Attribute)
	{
		if (!Attributes.IsNullOrEmpty() && Attributes.HasDelimitedSubstring(',', Attribute))
		{
			return true;
		}
		return false;
	}
}
