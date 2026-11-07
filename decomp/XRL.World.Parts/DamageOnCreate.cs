using System;
using UnityEngine;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DamageOnCreate : IPart
{
	public int Min = -1;

	public int Max = -1;

	public int MinPercent = -1;

	public int MaxPercent = -1;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (Min != -1 && Max != -1)
		{
			int hitpoints = Stat.Random(Min, Max);
			ParentObject.hitpoints = hitpoints;
		}
		else if (MinPercent != -1 && MaxPercent != -1)
		{
			float num = (float)Stat.Random(MinPercent, MaxPercent) / 100f;
			int baseValue = ParentObject.GetStat("Hitpoints").BaseValue;
			ParentObject.hitpoints = Math.Max(Mathf.RoundToInt((float)baseValue * num), 1);
		}
		return base.HandleEvent(E);
	}
}
