using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ModGlassArmor : IModification
{
	public string Type = "glass";

	public ModGlassArmor()
	{
	}

	public ModGlassArmor(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnWearer = true;
	}

	public override int GetModificationSlotUsage()
	{
		return 0;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<Armor>();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Damage.Amount > 0 && !E.Damage.HasAttribute("reflected") && !E.Damage.HasAttribute("Unavoidable") && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			GameObject equipped = ParentObject.Equipped;
			int num = (int)Math.Ceiling((float)E.Damage.Amount * (float)Tier / 100f);
			if (num > 0 && E.Actor != null && E.Actor != ParentObject && E.Actor != equipped)
			{
				List<string> list = new List<string>(E.Damage.Attributes);
				if (!list.Contains("reflected"))
				{
					list.Add("reflected");
				}
				if (equipped != null && equipped.IsPlayer())
				{
					string[] array = new string[6];
					GameObject parentObject = ParentObject;
					GameObject asPossessedBy = equipped;
					array[0] = parentObject.Does("reflect", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: false, SecondPerson: true, null, AsPossessed: true, asPossessedBy);
					array[1] = " ";
					array[2] = num.ToString();
					array[3] = " damage back at ";
					array[4] = E.Actor.t();
					array[5] = ".";
					IComponent<GameObject>.AddPlayerMessage(string.Concat(array));
				}
				E.Actor.TakeDamage(num, Attributes: string.Join(" ", list.ToArray()), Attacker: equipped ?? ParentObject, Source: ParentObject, Message: "from %t " + Type + " armor!");
				ParentObject.Equipped?.FireEvent("ReflectedDamage");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Reflects " + Tier + "% damage back at your attackers, rounded up.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("glass", 10);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
