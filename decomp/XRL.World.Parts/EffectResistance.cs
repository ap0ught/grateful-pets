using System;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class EffectResistance : IPart
{
	public string[] Effects;

	public int Chance = 100;

	public string Values
	{
		set
		{
			if (!value.IsNullOrEmpty())
			{
				Effects = value.Split(',');
			}
		}
	}

	public EffectResistance()
	{
		Effects = new string[0];
	}

	public EffectResistance(params string[] Effects)
	{
		this.Effects = Effects;
	}

	public override bool SameAs(IPart p)
	{
		if (p is EffectResistance effectResistance)
		{
			return Enumerable.SequenceEqual(effectResistance.Effects, Effects);
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanApplyEffectEvent.ID && ID != ApplyEffectEvent.ID)
		{
			return ID == SingletonEvent<GetDebugInternalsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanApplyEffectEvent E)
	{
		if (Chance >= 100 && Effects.Contains(E.Name))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ApplyEffectEvent E)
	{
		if (Effects.Contains(E.Name) && Chance.in100())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Resists", string.Join(", ", Effects));
		E.AddEntry(this, "Chance", Chance);
		return base.HandleEvent(E);
	}
}
