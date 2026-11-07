using System;
using System.Text;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class CleaveOnHit : IPart
{
	public int Chance = 75;

	public bool CriticalOnly;

	public CleaveOnHit()
	{
	}

	public CleaveOnHit(int Chance)
		: this()
	{
		this.Chance = Chance;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		if (Chance > 0)
		{
			if (Chance < 100)
			{
				SB.Append(Chance).Append("% chance to cleave ");
			}
			else
			{
				SB.Append("Cleaves ");
			}
			if (ParentObject.TryGetPart<MeleeWeapon>(out var Part) && Statistic.IsMental(Part.Stat))
			{
				SB.Append("mental ");
			}
			SB.Append("armor");
			if (CriticalOnly)
			{
				SB.Compound("on critical hit");
			}
		}
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			bool flag = E.HasFlag("Critical");
			if (CriticalOnly && !flag)
			{
				return base.FireEvent(E);
			}
			Axe_Cleave.PerformCleave(E.GetGameObjectParameter("Attacker"), E.GetGameObjectParameter("Defender"), ParentObject, null, Chance: (!CriticalOnly & flag) ? 100 : Chance, Properties: E.GetStringParameter("Properties"));
		}
		return base.FireEvent(E);
	}
}
