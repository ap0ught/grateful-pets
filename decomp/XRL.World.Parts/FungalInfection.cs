using System;
using Qud.API;
using XRL.Language;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FungalInfection : IPart
{
	public bool Curable = true;

	public override bool SameAs(IPart p)
	{
		if ((p as FungalInfection).Curable != Curable)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != PooledEvent<IsAfflictionEvent>.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsAfflictionEvent E)
	{
		return false;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "Eating");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "Eating");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AppliedLiquidCovered");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AppliedLiquidCovered")
		{
			GameObject equipped = ParentObject.Equipped;
			if (equipped != null)
			{
				if (IComponent<GameObject>.TheGame.GetStringGameState("FungalCureLiquid") == "")
				{
					IComponent<GameObject>.TheCore.GenerateFungalCure();
				}
				string liquidID = "gel";
				string stringGameState = IComponent<GameObject>.TheGame.GetStringGameState("FungalCureLiquid");
				LiquidVolume liquidVolume = E.GetParameter("Liquid") as LiquidVolume;
				if (liquidVolume.ContainsLiquid(liquidID) && liquidVolume.ContainsLiquid(stringGameState))
				{
					if (!Curable)
					{
						IComponent<GameObject>.EmitMessage(equipped, ParentObject.Does("are") + " immune to conventional treatments.", ' ', FromDialog: false, UsePopup: true);
					}
					else if (!equipped.HasEffect(typeof(FungalCureQueasy)))
					{
						IComponent<GameObject>.EmitMessage(equipped, equipped.Poss("skin") + " itches furiously.", ' ', FromDialog: false, UsePopup: true);
					}
					else
					{
						Cure();
					}
				}
			}
		}
		else if (E.ID == "Eating")
		{
			string stringGameState2 = IComponent<GameObject>.TheGame.GetStringGameState("FungalCureWorm");
			if (stringGameState2.IsNullOrEmpty())
			{
				IComponent<GameObject>.TheCore.GenerateFungalCure();
				stringGameState2 = IComponent<GameObject>.TheGame.GetStringGameState("FungalCureWorm");
			}
			GameObject gameObjectParameter = E.GetGameObjectParameter("Food");
			if (gameObjectParameter != null && gameObjectParameter.Blueprint == stringGameState2)
			{
				GameObject equipped2 = ParentObject.Equipped;
				if (equipped2 != null && !equipped2.HasEffect(typeof(FungalCureQueasy)))
				{
					equipped2.ApplyEffect(new FungalCureQueasy(100));
				}
			}
		}
		return base.FireEvent(E);
	}

	public bool Cure()
	{
		GameObject equipped = ParentObject.Equipped;
		string text = ParentObject.EquippedOn()?.GetOrdinalName() ?? "body";
		if (!ParentObject.Destroy(null, Silent: true))
		{
			return false;
		}
		if (equipped != null)
		{
			IComponent<GameObject>.EmitMessage(equipped, "The infected crust of skin on " + equipped.poss(text) + " loosens and breaks away.", ' ', FromDialog: false, UsePopup: true);
			if (equipped.IsPlayer())
			{
				JournalAPI.AddAccomplishment("To the dismay of fungi everywhere, you cured the " + ParentObject.DisplayNameOnlyStripped + " infection on your " + text + ".", "Bless the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", when =name= dissolved a sham alliance with the treacherous fungi by curing the " + ParentObject.DisplayNameOnlyStripped + " infection on " + equipped.GetPronounProvider().PossessiveAdjective + " " + text + ".", "While traveling around " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= stumbled upon a clan of fungi performing a secret ritual. Because of  " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " <spice.elements." + The.Player.GetMythicDomain() + ".quality.!random>, they furiously rebuked " + The.Player.GetPronounProvider().Objective + " and dissolved the " + ParentObject.DisplayNameOnlyStripped + " on " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " " + text + ".", null, "general", MuralCategory.BodyExperienceNeutral, MuralWeight.Medium, null, -1L);
			}
		}
		return true;
	}
}
