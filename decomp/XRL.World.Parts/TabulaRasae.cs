using System;
using System.Collections.Generic;
using XRL.Messages;

namespace XRL.World.Parts;

[Serializable]
public class TabulaRasae : IPart
{
	public static readonly string[] EXEMPT = new string[3] { "AffectGas", "Unavoidable", "NonPenetrating" };

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeApplyDamageEvent.ID)
		{
			return ID == TookDamageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeApplyDamageEvent E)
	{
		if (E.Object == ParentObject && E.Damage.HasAnyAttribute(GetDamageImmunities()))
		{
			if (E.Actor != null && E.Actor.IsPlayer())
			{
				MessageQueue.AddPlayerMessage("Your attack does not affect " + ParentObject.t() + ".");
			}
			NotifyTargetImmuneEvent.Send(E.Weapon, E.Object, E.Actor, E.Damage, this);
			E.Damage.Amount = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TookDamageEvent E)
	{
		if (E.Object == ParentObject && ParentObject.hitpoints <= 0)
		{
			List<string> damageImmunities = GetDamageImmunities();
			if (E.Damage != null)
			{
				foreach (string attribute in E.Damage.Attributes)
				{
					if (!damageImmunities.Contains(attribute))
					{
						damageImmunities.Add(attribute);
						MessageQueue.AddPlayerMessage("The Tabula Rasae adapt to " + attribute.ToLower() + " damage.");
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public List<string> GetDamageImmunities()
	{
		if (!The.Game.HasGameState("TabulaRasaeDamageImmunities"))
		{
			The.Game.SetObjectGameState("TabulaRasaeDamageImmunities", new List<string>());
		}
		return The.Game.GetObjectGameState("TabulaRasaeDamageImmunities") as List<string>;
	}
}
