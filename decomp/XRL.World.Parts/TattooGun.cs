using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class TattooGun : IPoweredPart
{
	public TattooGun()
	{
		ChargeUse = 0;
		ConsumesLiquid = "ink";
		MustBeUnderstood = true;
		WorksOnEquipper = true;
		WorksOnCarrier = true;
		IsEMPSensitive = false;
		NameForStatus = "IntradermalInjector";
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
			E.AddAction("Tattoo", "tattoo", "Tattoo", null, 't', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Tattoo" && AttemptTattoo(E.Actor))
		{
			E.Actor.UseEnergy(10000, "Item TattooGun");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public bool AttemptTattoo(GameObject Actor)
	{
		if (!Actor.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		if (Actor.AreHostilesNearby())
		{
			Actor.Fail("You can't tattoo with hostiles nearby.");
			return false;
		}
		ActivePartStatus activePartStatus = GetActivePartStatus(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L);
		if (activePartStatus != ActivePartStatus.Operational)
		{
			switch (activePartStatus)
			{
			case ActivePartStatus.ProcessInputMissing:
				Actor.Fail(ParentObject.Does("are") + " out of {{|" + LiquidVolume.GetLiquid(ConsumesLiquid).GetName() + "}}.");
				break;
			case ActivePartStatus.ProcessInputInvalid:
				Actor.Fail(ParentObject.Does("are") + " filled with contaminated {{|" + LiquidVolume.GetLiquid(ConsumesLiquid).GetName() + "}}!");
				break;
			case ActivePartStatus.Unpowered:
				Actor.Fail(ParentObject.T() + " merely" + ParentObject.GetVerb("click") + ".");
				break;
			default:
				Actor.Fail(ParentObject.Does("aren't") + " working!");
				break;
			}
			return false;
		}
		if (!Actor.IsPlayer())
		{
			return false;
		}
		if (Actor.CurrentCell == null)
		{
			return false;
		}
		Cell cell = PickDirection("Tattoo");
		if (cell == null)
		{
			return false;
		}
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("Body", Actor.GetPhase(), Actor, null, null, null, CheckFlight: true);
		if (firstObjectWithPart == null)
		{
			Actor.Fail("There is nobody there you can tattoo.");
			return false;
		}
		if (firstObjectWithPart != Actor && !firstObjectWithPart.IsPlayerLed())
		{
			Actor.Fail("You can only tattoo " + Actor.itself + " and your companions.");
			return false;
		}
		Body body = firstObjectWithPart.Body;
		if (body == null)
		{
			Actor.Fail("You cannot tattoo " + ((firstObjectWithPart == Actor) ? Actor.itself : firstObjectWithPart.t()) + ".");
			return false;
		}
		List<BodyPart> list = new List<BodyPart>();
		foreach (BodyPart part in body.GetParts())
		{
			if (Tattoos.GetBodyPartGeneralTattooability(part) == Tattoos.ApplyResult.Tattooable)
			{
				list.Add(part);
			}
		}
		if (list.Count == 0)
		{
			Actor.Fail("You cannot tattoo " + ((firstObjectWithPart == Actor) ? Actor.itself : firstObjectWithPart.t()) + ".");
			return false;
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
		int num = Popup.PickOption("Choose a body part to tattoo.", null, "", "Sounds/UI/ui_notification", list2.ToArray(), list4.ToArray(), null, null, null, null, null, 0, 60, 0, -1, AllowEscape: true);
		if (num < 0)
		{
			return false;
		}
		BodyPart bodyPart = list3[num];
		if (bodyPart == null)
		{
			return false;
		}
		Tattoos.ApplyResult applyResult = Tattoos.CanApplyTattoo(Actor, bodyPart, CanTattoo: true, CanEngrave: false);
		if (applyResult != Tattoos.ApplyResult.Tattooable)
		{
			switch (applyResult)
			{
			case Tattoos.ApplyResult.TooManyTattoos:
				Actor.Fail("There are too many tattoos on " + ((firstObjectWithPart == Actor) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + " to add more.");
				break;
			case Tattoos.ApplyResult.AbstractBodyPart:
			case Tattoos.ApplyResult.NonContactBodyPart:
				Actor.Fail("You can't tattoo " + ((firstObjectWithPart == Actor) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + ".");
				break;
			case Tattoos.ApplyResult.InappropriateBodyPart:
				Actor.Fail(((firstObjectWithPart == Actor) ? "Your" : Grammar.MakePossessive(firstObjectWithPart.T())) + " " + bodyPart.GetOrdinalName() + " can't be tattooed because " + (bodyPart.Plural ? "they don't" : "it doesn't") + " have flesh.");
				break;
			default:
				Actor.Fail("You can't tattoo " + ((firstObjectWithPart == Actor) ? "your" : Grammar.MakePossessive(firstObjectWithPart.t())) + " " + bodyPart.GetOrdinalName() + ".");
				break;
			}
			return false;
		}
		string text = Popup.AskString("Describe your tattoo. For example: \"a tiny snail\".", "", "Sounds/UI/ui_notification", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -+/#()!@$%*<>", null, 30);
		if (text.IsNullOrEmpty())
		{
			return false;
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			Actor.Fail(ParentObject.Does("aren't") + " working!");
			return false;
		}
		string text2 = Popup.ShowColorPicker("Choose a primary color.", 0, null, 60, RespectOptionNewlines: false, AllowEscape: false, null, "", includeNone: true, includePatterns: false, allowBackground: true);
		if (text2 == null)
		{
			return false;
		}
		string text3 = null;
		if (!text2.IsNullOrEmpty())
		{
			text3 = Popup.ShowColorPicker("Choose a secondary color.");
			if (text3 == null)
			{
				return false;
			}
		}
		bool flag = Actor.HasMarkOfDeath();
		if (!Tattoos.IsSuccess(Tattoos.ApplyTattoo(firstObjectWithPart, bodyPart, CanTattoo: true, CanEngrave: false, text, text2, text3)))
		{
			Actor.Fail("Something went wrong and the tattooing fails.");
			return false;
		}
		Actor.PlayWorldOrUISound("Sounds/Interact/sfx_interact_tattoo");
		ConsumeCharge();
		if (Actor.IsPlayer())
		{
			if (!flag && Actor.HasMarkOfDeath())
			{
				Popup.Show("You tattoo the mark of death on " + firstObjectWithPart.poss(bodyPart.GetOrdinalName()) + ".");
				The.Game.FinishQuestStep("Tomb of the Eaters", "Inscribe the Mark");
			}
			else
			{
				Popup.Show("You tattoo " + text + " on " + firstObjectWithPart.poss(bodyPart.GetOrdinalName()) + ".");
			}
			if (firstObjectWithPart == Actor)
			{
				Achievement.TATTOO_SELF.Unlock();
			}
		}
		return true;
	}
}
