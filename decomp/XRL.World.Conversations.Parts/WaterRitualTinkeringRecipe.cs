using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Conversations.Parts;

public class WaterRitualTinkeringRecipe : IWaterRitualPart
{
	public TinkerData Data;

	public override void Awake()
	{
		if (WaterRitual.Record.numBlueprints <= 0)
		{
			return;
		}
		List<TinkerData> tinkerRecipes = TinkerData.TinkerRecipes;
		if (WaterRitual.Record.tinkerdata.IsNullOrEmpty())
		{
			Random random = new Random(WaterRitual.Record.mySeed);
			int num = WaterRitual.Record.numBlueprints;
			int num2 = 100;
			while (num2 > 0 && num > 0)
			{
				int item;
				if (!tinkerRecipes[item = random.Next(tinkerRecipes.Count)].Known() && !WaterRitual.Record.tinkerdata.Contains(item))
				{
					WaterRitual.Record.tinkerdata.Add(item);
					num--;
				}
				num2--;
			}
		}
		foreach (int tinkerdatum in WaterRitual.Record.tinkerdata)
		{
			Data = tinkerRecipes[tinkerdatum];
			if (!Data.Known())
			{
				Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "TinkerRecipe", 50 * Data.Tier / 3);
				Visible = true;
				break;
			}
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != IsElementVisibleEvent.ID && ID != PrepareTextEvent.ID && ID != GetChoiceTagEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		if (Data.Type == "Mod")
		{
			E.Text.Replace("=recipe=", "[{{W|Item mod}}] - {{C|" + Data.DisplayName + "}}");
		}
		else
		{
			GameObject gameObject = GameObject.CreateSample(Data.Blueprint);
			E.Text.Replace("=recipe=", gameObject.IsPluralIfKnown ? gameObject.DisplayNameOnlyDirect : Grammar.Pluralize(gameObject.DisplayNameOnlyDirect));
			gameObject.Obliterate();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (UseReputation())
		{
			TinkerData.KnownRecipes.Add(Data);
			WaterRitual.Record.numBlueprints--;
			The.Listener.PlayWorldOrUISound("sfx_characterMod_tinkerSchematic_learn");
			if (Data.Type == "Mod")
			{
				Popup.Show(The.Speaker.Does("teach", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you to craft the item modification {{W|" + Data.DisplayName + "}}.");
			}
			else
			{
				GameObject gameObject = GameObject.CreateSample(Data.Blueprint);
				gameObject.MakeUnderstood();
				Popup.Show(The.Speaker.Does("teach", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you to craft " + (gameObject.IsPlural ? Data.DisplayName : Grammar.Pluralize(gameObject.DisplayNameOnlyDirectAndStripped)) + ".");
				gameObject.Obliterate();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
		return false;
	}
}
