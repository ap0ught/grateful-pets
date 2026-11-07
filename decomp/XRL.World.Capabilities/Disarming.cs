using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace XRL.World.Capabilities;

public static class Disarming
{
	public static GameObject Disarm(GameObject Subject, GameObject Disarmer, int SaveTarget, string SaveStat = "Strength", string DisarmerStat = "Agility", GameObject Weapon = null, GameObject DisarmingWeapon = null)
	{
		if (Weapon == null)
		{
			Body body = Subject.Body;
			if (body == null)
			{
				return null;
			}
			List<GameObject> list = Event.NewGameObjectList();
			foreach (BodyPart part in body.GetParts())
			{
				GameObject equipped = part.Equipped;
				if (equipped != null && !list.Contains(equipped) && equipped.IsReal && equipped.CanBeUnequipped(null, null, Forced: false, SemiForced: true) && (part.Type == "Missile Weapon" || (equipped.TryGetPart<MeleeWeapon>(out var Part) && Part.AttackFromPart(part))))
				{
					list.Add(equipped);
				}
			}
			Weapon = list.GetRandomElement();
			if (Weapon == null)
			{
				return null;
			}
		}
		else if (!Weapon.CanBeUnequipped(null, null, Forced: false, SemiForced: true))
		{
			return null;
		}
		try
		{
			if (!Subject.MakeSave(SaveStat, SaveTarget, Disarmer, DisarmerStat, "Disarm", IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, DisarmingWeapon) && Subject.FireEvent(Event.New("CommandForceUnequipObject", "Object", Weapon).SetSilent(Silent: true)))
			{
				Messaging.WDidXToYWithZ(Disarmer, "disarm", null, Subject, "of", Weapon, null, "!", null, null, null, Subject, UseFullNames: false, IndefiniteSubject: false, IndefiniteDirectObject: false, IndefiniteIndirectObject: false, IndefiniteDirectObjectForOthers: false, IndefiniteIndirectObjectForOthers: false, PossessiveDirectObject: false, PossessiveIndirectObject: false, null, null, Subject, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: false, Subject.IsPlayerControlled());
				if (InventoryActionEvent.Check(Subject, Subject, Weapon, "CommandDropObject", Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, Forced: true, Silent: true))
				{
					Weapon.Move(Directions.GetRandomDirection(), Forced: true);
				}
				return Weapon;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Error when disarming " + Weapon.DebugName + " from " + Subject.DebugName, x);
		}
		return null;
	}
}
