using System;
using System.Text;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class ThrownWeapon : IPart
{
	public string Damage = "1d2";

	public int Penetration = 1;

	public int PenetrationBonus;

	public string Attributes;

	public override bool SameAs(IPart p)
	{
		ThrownWeapon thrownWeapon = p as ThrownWeapon;
		if (thrownWeapon.Damage != Damage)
		{
			return false;
		}
		if (thrownWeapon.Penetration != Penetration)
		{
			return false;
		}
		if (thrownWeapon.PenetrationBonus != PenetrationBonus)
		{
			return false;
		}
		if (thrownWeapon.Attributes != Attributes)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == QueryEquippableListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.SlotType == "Thrown Weapon" && E.Item == ParentObject && !E.List.Contains(E.Item))
		{
			if (!E.RequirePossible)
			{
				E.List.Add(E.Item);
			}
			else if (E.Actor.IsGiganticCreature)
			{
				if (E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
				{
					E.List.Add(E.Item);
				}
			}
			else if (!E.Item.IsGiganticEquipment || E.Item.IsNatural())
			{
				E.List.Add(E.Item);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !ParentObject.HasTagOrProperty("HideThrownWeaponPerformance"))
		{
			BodyPart bodyPart = ParentObject.EquippedOn();
			MeleeWeapon Part;
			if (bodyPart == null || bodyPart.Type == "Thrown Weapon")
			{
				E.AddTag(GetPerformanceTag());
			}
			else if (!ParentObject.HasTagOrProperty("ShowMeleeWeaponStats") && ParentObject.TryGetPart<MeleeWeapon>(out Part))
			{
				E.AddTag(Part.GetSimplifiedStats(Options.ShowDetailedWeaponStats));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GenericQueryEvent E)
	{
		if (E.Query == "PhaseHarmonicEligible")
		{
			E.Result = true;
		}
		return base.HandleEvent(E);
	}

	public string GetPerformanceTag()
	{
		GameObject equipped = ParentObject.Equipped;
		string Damage = this.Damage;
		int Penetration = this.Penetration;
		int PenetrationBonus = this.PenetrationBonus;
		int PenetrationModifier = equipped?.Stat("Strength") ?? 0;
		bool Vorpal = Attributes != null && Attributes.HasDelimitedSubstring(' ', "Vorpal");
		GetThrownWeaponPerformanceEvent.GetFor(ParentObject, ref Damage, ref Penetration, ref PenetrationBonus, ref PenetrationModifier, ref Vorpal, Prospective: true, equipped, equipped?.Target);
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("{{c|").Append('\u001a').Append("}}");
		if (Vorpal)
		{
			stringBuilder.Append('÷');
			if (PenetrationBonus != 0)
			{
				stringBuilder.Append(PenetrationBonus.Signed());
			}
		}
		else
		{
			stringBuilder.Append(Math.Min(PenetrationModifier, Penetration) + PenetrationBonus + RuleSettings.VISUAL_PENETRATION_BONUS);
		}
		stringBuilder.Append(" {{r|").Append('\u0003').Append("}}")
			.Append(Damage);
		return stringBuilder.ToString();
	}

	public bool AdjustDamageDieSize(int Amount)
	{
		Damage = DieRoll.AdjustDieSize(Damage, Amount);
		DamageDieSizeAdjustedEvent.Send(ParentObject, this, Amount);
		return true;
	}

	public bool AdjustDamage(int Amount)
	{
		Damage = DieRoll.AdjustResult(Damage, Amount);
		DamageConstantAdjustedEvent.Send(ParentObject, this, Amount);
		return true;
	}
}
