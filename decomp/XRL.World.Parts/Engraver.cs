using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class Engraver : IPoweredPart
{
	public Engraver()
	{
		ChargeUse = 500;
		WorksOnEquipper = true;
		NameForStatus = "PrecisionEngraver";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (IsObjectActivePartSubject(E.Actor))
		{
			E.AddAction("Engrave", "engrave", "Engrave", null, 'e', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Engrave" && AttemptEngrave(E.Actor))
		{
			E.Actor.UseEnergy(10000, "Item Engraver");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public bool AttemptEngrave(GameObject Actor)
	{
		if (!Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		if (Actor.AreHostilesNearby())
		{
			return Actor.Fail("You can't engrave with hostiles nearby.");
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		switch (activePartStatus)
		{
		case ActivePartStatus.Unpowered:
			return Actor.Fail(ParentObject.Does("click", int.MaxValue, null, null, "merely") + ".");
		default:
			return Actor.Fail(ParentObject.Does("are") + " " + GetStatusPhrase(activePartStatus) + ".");
		case ActivePartStatus.Operational:
		{
			if (!Actor.IsPlayer())
			{
				return false;
			}
			if (Actor.CurrentCell == null)
			{
				return false;
			}
			Cell cell = PickDirection("Engrave");
			if (cell == null)
			{
				return false;
			}
			GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("Body", Actor.GetPhase(), Actor, null, null, null, CheckFlight: true);
			if (firstObjectWithPart == null)
			{
				return Actor.Fail("There is nobody there you can engrave.");
			}
			if (firstObjectWithPart != Actor && !firstObjectWithPart.IsPlayerLed())
			{
				return Actor.Fail("You can only engrave " + Actor.itself + " and your companions.");
			}
			Body body = firstObjectWithPart.Body;
			if (body == null)
			{
				return Actor.Fail("You cannot engrave " + ((firstObjectWithPart == Actor) ? Actor.itself : firstObjectWithPart.t()) + ".");
			}
			List<BodyPart> list = new List<BodyPart>();
			foreach (BodyPart part in body.GetParts())
			{
				if (Tattoos.GetBodyPartGeneralTattooability(part) == Tattoos.ApplyResult.Engravable)
				{
					list.Add(part);
				}
			}
			if (list.Count == 0)
			{
				return Actor.Fail("You cannot engrave " + ((firstObjectWithPart == Actor) ? Actor.itself : firstObjectWithPart.t()) + ".");
			}
			List<string> list2 = new List<string>(list.Count);
			List<BodyPart> list3 = new List<BodyPart>(list.Count);
			List<char> list4 = new List<char>(list.Count);
			char c = 'a';
			foreach (BodyPart item in list)
			{
				list2.Add(item.GetCardinalName());
				list3.Add(item);
				list4.Add(c);
				c = (char)(c + 1);
			}
			int num = Popup.PickOption("Choose a body part to engrave.", null, "", "Sounds/UI/ui_notification", list2.ToArray(), list4.ToArray(), null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
			if (num < 0)
			{
				return false;
			}
			BodyPart bodyPart = list3[num];
			if (bodyPart == null)
			{
				return false;
			}
			switch (Tattoos.CanApplyTattoo(Actor, bodyPart, CanTattoo: false, CanEngrave: true))
			{
			case Tattoos.ApplyResult.TooManyTattoos:
				return Actor.Fail("There are too many engravings on " + ((firstObjectWithPart == Actor) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + " to add more.");
			case Tattoos.ApplyResult.AbstractBodyPart:
			case Tattoos.ApplyResult.NonContactBodyPart:
				return Actor.Fail("You can't engrave " + ((firstObjectWithPart == Actor) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + ".");
			case Tattoos.ApplyResult.InappropriateBodyPart:
				return Actor.Fail(((firstObjectWithPart == Actor) ? "Your" : Grammar.MakePossessive(firstObjectWithPart.T())) + " " + bodyPart.GetOrdinalName() + " can't be engraved because " + (bodyPart.Plural ? "they don't" : "it doesn't") + " have a hard enough body part.");
			default:
				return Actor.Fail("You can't engrave " + ((firstObjectWithPart == Actor) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + ".");
			case Tattoos.ApplyResult.Engravable:
			{
				string text = Popup.AskString("Describe your engraving. For example: \"a tiny snail\".", "", "Sounds/UI/ui_notification", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -+/#()!@$%*<>", null, 30);
				if (text.IsNullOrEmpty())
				{
					return false;
				}
				bool flag = Actor.HasMarkOfDeath();
				if (!Tattoos.IsSuccess(Tattoos.ApplyTattoo(firstObjectWithPart, bodyPart, CanTattoo: false, CanEngrave: true, text)))
				{
					return Actor.Fail("Something went wrong and the engraving fails.");
				}
				PlayWorldSound("Sounds/Interact/sfx_interact_engraver");
				ConsumeCharge();
				if (Actor.IsPlayer())
				{
					if (!flag && Actor.HasMarkOfDeath())
					{
						Popup.Show("You engrave the mark of death on " + firstObjectWithPart.poss(bodyPart.GetOrdinalName()) + ".");
						The.Game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
					}
					else
					{
						Popup.Show("You engrave " + text + " on " + firstObjectWithPart.poss(bodyPart.GetOrdinalName()) + ".");
					}
				}
				return true;
			}
			}
		}
		}
	}
}
