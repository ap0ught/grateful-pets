using System;

namespace XRL.World.Parts;

[Serializable]
public class MountedFurniture : IPart
{
	public bool Mounted = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AfterObjectCreatedEvent.ID || !Mounted) && (ID != PooledEvent<AnimateEvent>.ID || !Mounted) && ID != ApplyEffectEvent.ID && ID != CanApplyEffectEvent.ID && ID != EffectAppliedEvent.ID && ID != EquipperEquippedEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetKineticResistanceEvent>.ID && ID != PooledEvent<IsRootedInPlaceEvent>.ID)
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AnimateEvent E)
	{
		Mounted = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (!Check(E))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (ParentObject.IsFlying)
		{
			Mounted = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquipperEquippedEvent E)
	{
		if (Mounted)
		{
			Armor armor = E.Item?.GetPart<Armor>();
			if (armor != null && (armor.WornOn == "Feet" || armor.WornOn == "Roots"))
			{
				Mounted = false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetKineticResistanceEvent E)
	{
		if (Mounted)
		{
			E.LinearIncrease += 300;
			E.PercentageIncrease += 300;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsRootedInPlaceEvent E)
	{
		if (Mounted)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		Mounted = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		if (ParentObject.IsPotentiallyMobile())
		{
			Mounted = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Mounted", Mounted);
		return base.HandleEvent(E);
	}

	private bool Check(IEffectCheckEvent E)
	{
		if (Mounted)
		{
			if (E.Name == "Prone")
			{
				return false;
			}
			if (E.Name == "Wading")
			{
				return false;
			}
			if (E.Name == "Swimming")
			{
				return false;
			}
		}
		return true;
	}
}
