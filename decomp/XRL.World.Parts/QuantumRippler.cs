using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class QuantumRippler : IPoweredPart
{
	public string Blueprint = "Space-Time Rift";

	public string TimeToGenerate = "3d10";

	public string TimeToStabilize = "3d10";

	public string StabilizeDuration = "1d6";

	public string Directions = "N,S,E,W";

	public int TimeLeftToGenerate;

	public int TimeLeftToStabilize;

	public bool ExplodeOnRealityStabilize = true;

	static QuantumRippler()
	{
	}

	public QuantumRippler()
	{
		WorksOnSelf = true;
		IsRealityDistortionBased = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID && (ID != PooledEvent<RealityStabilizeEvent>.ID || !ExplodeOnRealityStabilize))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		CheckDirections();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		CheckDirections(Initial: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (ExplodeOnRealityStabilize && E.Check(CanDestroy: true))
		{
			ParentObject.ParticleBlip("&K!", 10, 0L);
			if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(ParentObject.Does("collapse") + " under the pressure of normality and" + ParentObject.GetVerb("implode") + ".");
			}
			ParentObject.Explode(20000, E.Effect.Owner, "12d10+300", 1f, Neutron: true);
			return false;
		}
		return base.HandleEvent(E);
	}

	public void CheckDirections(bool Initial = false)
	{
		if (Directions.IsNullOrEmpty())
		{
			return;
		}
		foreach (string item in Directions.CachedCommaExpansion())
		{
			CheckDirection(item, Initial);
		}
	}

	public void CheckDirection(string Direction, bool Initial = false)
	{
		if (Blueprint.IsNullOrEmpty())
		{
			return;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		Cell cellFromDirection = cell.GetCellFromDirection(Direction);
		if (cellFromDirection == null)
		{
			return;
		}
		Cell cellFromDirection2 = cellFromDirection.GetCellFromDirection(Direction);
		if (cellFromDirection2 == null)
		{
			return;
		}
		GameObject firstObjectWithPart = cellFromDirection2.GetFirstObjectWithPart(Name);
		if (firstObjectWithPart == null || !IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		QuantumRippler part = firstObjectWithPart.GetPart<QuantumRippler>();
		if (part == null || !part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		while (true)
		{
			GameObject firstObject = cellFromDirection.GetFirstObject(Blueprint);
			if (firstObject != null)
			{
				QuantumStabilized effect = firstObject.GetEffect<QuantumStabilized>();
				if (effect == null)
				{
					if (!TimeToStabilize.IsNullOrEmpty())
					{
						if (Initial)
						{
							TimeLeftToStabilize = 0;
						}
						else
						{
							TimeLeftToStabilize--;
						}
						if (TimeLeftToStabilize < 0)
						{
							TimeLeftToStabilize = TimeToStabilize.RollCached();
						}
						else if (TimeLeftToStabilize == 0)
						{
							firstObject.ForceApplyEffect(new QuantumStabilized(StabilizeDuration.RollCached()));
						}
						break;
					}
				}
				else if (!StabilizeDuration.IsNullOrEmpty())
				{
					int num = StabilizeDuration.RollCached();
					if (effect.Duration < num)
					{
						effect.Duration = num;
					}
					break;
				}
				return;
			}
			if (TimeToGenerate.IsNullOrEmpty())
			{
				return;
			}
			if (Initial)
			{
				TimeLeftToGenerate = 0;
			}
			else
			{
				TimeLeftToGenerate--;
			}
			if (TimeLeftToGenerate < 0)
			{
				TimeLeftToGenerate = TimeToGenerate.RollCached();
				break;
			}
			if (TimeLeftToGenerate != 0)
			{
				break;
			}
			cellFromDirection.AddObject(Blueprint);
			if (!Initial)
			{
				break;
			}
			Initial = false;
		}
		ConsumeCharge();
	}
}
