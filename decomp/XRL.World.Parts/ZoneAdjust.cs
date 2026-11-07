using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ZoneAdjust : IPoweredPart
{
	public string AdjustSpec;

	public int Duration = 2;

	public string VariableDuration;

	public string RequiresBlueprint;

	public string RequiresEffect;

	public string RequiresPart;

	public string RequiresTag;

	public string NoAdjustSpecFailureDescription;

	public ZoneAdjust()
	{
		WorksOnSelf = true;
	}

	public bool Affects(GameObject obj)
	{
		if (RequiresPart != null && !obj.HasPart(RequiresPart))
		{
			return false;
		}
		if (RequiresEffect != null && !obj.HasEffect(RequiresEffect))
		{
			return false;
		}
		if (RequiresTag != null && !obj.HasTagOrProperty(RequiresTag))
		{
			return false;
		}
		if (RequiresBlueprint != null && !RequiresBlueprint.CachedCommaExpansion().Contains(obj.Blueprint))
		{
			return false;
		}
		return true;
	}

	public override bool SameAs(IPart p)
	{
		ZoneAdjust zoneAdjust = p as ZoneAdjust;
		if (zoneAdjust.AdjustSpec != AdjustSpec)
		{
			return false;
		}
		if (zoneAdjust.Duration != Duration)
		{
			return false;
		}
		if (zoneAdjust.VariableDuration != VariableDuration)
		{
			return false;
		}
		if (zoneAdjust.RequiresBlueprint != RequiresBlueprint)
		{
			return false;
		}
		if (zoneAdjust.RequiresEffect != RequiresEffect)
		{
			return false;
		}
		if (zoneAdjust.RequiresPart != RequiresPart)
		{
			return false;
		}
		if (zoneAdjust.RequiresTag != RequiresTag)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (!AdjustSpec.IsNullOrEmpty() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && cell.ParentZone != null)
			{
				int duration = (VariableDuration.IsNullOrEmpty() ? Duration : Stat.Roll(VariableDuration));
				Adjusted source = new Adjusted(AdjustSpec, duration, ParentObject);
				foreach (GameObject @object in cell.ParentZone.GetObjects(Affects))
				{
					bool flag = false;
					foreach (Effect effect in @object.Effects)
					{
						if (effect is Adjusted adjusted && ParentObject.IDMatch(adjusted.SourceID))
						{
							adjusted.Duration = 0;
							flag = true;
						}
					}
					if (flag)
					{
						@object.CleanEffects();
					}
					@object.ApplyEffect(new Adjusted(source));
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return AdjustSpec.IsNullOrEmpty();
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		if (AdjustSpec.IsNullOrEmpty() && !NoAdjustSpecFailureDescription.IsNullOrEmpty())
		{
			return NoAdjustSpecFailureDescription;
		}
		return null;
	}
}
