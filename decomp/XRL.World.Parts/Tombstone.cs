using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using HistoryKit;
using Qud.API;
using XRL.Names;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Tombstone : IPart
{
	public string Inscription;

	public bool NeedsGeneration = true;

	public string faction;

	public string name;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Look.ShowLooker(0, cell.X, cell.Y);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (NeedsGeneration)
		{
			GenerateTombstone();
		}
		E.Prefix.Append(Inscription);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.Render.Tile = "terrain/sw_tombstone_" + Stat.Random(1, 4) + ".bmp";
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void GenerateTombstone()
	{
		if (name == null)
		{
			name = NameMaker.MakeName(null, null, null, null, null, faction);
		}
		string input = HistoricStringExpander.ExpandString("<spice.tombstones.intro.!random>");
		string text = "";
		GameObject aNonLegendaryCreature = EncountersAPI.GetANonLegendaryCreature();
		int num = Stat.Random(0, 850);
		if (num < 100)
		{
			text = new string[8] { "Stabbed to death", "Shanked", "Gunned down", "Poisoned", "Pushed off a cliff", "Tricked into jumping into a pool of lava", "Drowned in a lake", "Thrown into a pool of acid" }.GetRandomElement() + " by " + aNonLegendaryCreature.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true);
		}
		else if (num < 200)
		{
			GameObject gameObject = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Item"));
			text = new string[2] { "Killed in a duel over ", "Succumbed to despair after losing " }.GetRandomElement();
			text += gameObject.an(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true);
			gameObject.Obliterate();
		}
		else if (num < 300)
		{
			GameObject gameObject2 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Item"));
			text = new string[2] { "Crushed to death by a falling ", "Fell from a cliff trying to recover a lost " }.GetRandomElement();
			text += gameObject2.GetDisplayName(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true);
			gameObject2.Obliterate();
		}
		else if (num < 400)
		{
			string randomFaction = GenerateFriendOrFoe.getRandomFaction(IComponent<GameObject>.ThePlayer);
			text = new string[16]
			{
				"Murdered under mysterious circumstances by ", "Eaten alive by ", "Sacrificed by ", "Assassinated after disparaging ", "Killed after cooking a rancid meal for ", "Burned at the stake by ", "Thrown from a cliff by ", " Buried alive by ", "Brained in retaliation for stealing from ", "Shanked in retaliation for stealing from ",
				"Shot in retaliation for stealing from ", "Cooked for sustenance by ", "Mummified by ", "Chopped into small pieces by ", "Covered in molten wax by ", "Drawn and quartered by "
			}.GetRandomElement() + Faction.GetFormattedName(randomFaction);
		}
		else if (num < 450)
		{
			GameObject gameObject3 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Food"));
			text = new string[3] { "Choked on ", "Ate too much ", "Died of malnutrition after eating only " }.GetRandomElement();
			text = ((!text.StartsWith("Choked")) ? (text + gameObject3.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, false)) : (text + gameObject3.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)));
			gameObject3.Obliterate();
		}
		else if (num < 475)
		{
			GameObject gameObject4 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Tonic"));
			text = new string[3] { "Swallowed ", "Overdosed on ", "Accidentally ingested " }.GetRandomElement() + gameObject4.an(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true);
			gameObject4.Obliterate();
		}
		else if (num < 500)
		{
			string[] list = new string[1] { "Injected one " };
			GameObject gameObject5 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Tonic"));
			text = string.Concat(str1: gameObject5.GetDisplayName(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true), str0: list.GetRandomElement(), str2: " too many");
			gameObject5.Obliterate();
		}
		else if (num < 525)
		{
			GameObject gameObject6 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Gas"));
			text = new string[4] { "Choked to death on ", "Suffocated in ", "Breathed too much ", "Released a canister of " }.GetRandomElement();
			text = ((!text.StartsWith("Choked")) ? (text + gameObject6.GetDisplayName(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true)) : (text + gameObject6.an(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true)));
			if (text.StartsWith("Released"))
			{
				text += " in a locked room";
			}
			gameObject6.Obliterate();
		}
		else if (num < 575)
		{
			GameObject gameObject7 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Grenade"));
			text = new string[4] { "Sat on ", "Swallowed ", "Forgot about ", "Knocked over " }.GetRandomElement() + gameObject7.an(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true);
			gameObject7.Obliterate();
		}
		else if (num < 600)
		{
			GameObject gameObject8 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Book"));
			string[] list2 = new string[2] { "Knocked over the head with a copy of ", "Burned at the stake for promulgating " };
			text = string.Concat(str1: gameObject8.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true), str0: list2.GetRandomElement());
			gameObject8.Obliterate();
		}
		else if (num < 625)
		{
			GameObject gameObject9 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("Book"));
			string displayName = gameObject9.GetDisplayName(int.MaxValue, null, null, AsIfKnown: true, Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true);
			text = "Became obsessed with " + displayName + " and forgot to eat";
			gameObject9.Obliterate();
		}
		else if (num < 675)
		{
			string[] list3 = new string[4] { "Burned to death by ", "Immolated in ", "Engulfed in flame by ", "Fell asleep on " };
			GameObject gameObject10 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("LightSource"));
			text = list3.GetRandomElement() + gameObject10.an(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true);
			gameObject10.Obliterate();
		}
		else if (num < 700)
		{
			string[] list4 = new string[1] { "Drank from a poisoned " };
			GameObject gameObject11 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("WaterContainer"));
			text = list4.GetRandomElement() + gameObject11.GetDisplayName(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true, ColorOnly: false, Visible: true, WithoutTitles: false, ForSort: false, Short: true, BaseOnly: false, WithIndefiniteArticle: false, WithDefiniteArticle: false, null, IndicateHidden: false, Capitalize: false, SecondPerson: false, Reflexive: false, true);
			gameObject11.Obliterate();
		}
		else if (num < 750)
		{
			string[] list5 = new string[1] { "Made too many mocking sounds at " };
			GameObject gameObject12 = GameObject.CreateSample(EncountersAPI.GetARandomDescendantOf("BaseApe"));
			text = list5.GetRandomElement() + gameObject12.an(int.MaxValue, null, null, 75.in100(), Single: false, NoConfusion: false, NoColor: false, Stripped: true);
			gameObject12.Obliterate();
		}
		else if (num < 800)
		{
			text = new string[2] { "Died of old age", "Died of natural causes" }.GetRandomElement();
		}
		else
		{
			string[] list6 = new string[9] { "natural", "unnatural", "mysterious", "metaphysical", "mathematical", "chemical", "ceremonial", "unknown", "magmatic" };
			text = "Died of " + list6.GetRandomElement() + " causes";
		}
		text = ((!string.IsNullOrEmpty(Inscription)) ? Markup.Wrap(Inscription) : Markup.Wrap(text));
		List<string> list7 = new List<string>(16);
		int maxWidth = 25;
		list7.Add("");
		string[] array = StringFormat.ClipText(input, maxWidth).Split('\n');
		foreach (string item in array)
		{
			list7.Add(item);
		}
		list7.Add("");
		list7.Add("");
		array = StringFormat.ClipText(name, maxWidth).Split('\n');
		foreach (string item2 in array)
		{
			list7.Add(item2);
		}
		list7.Add("");
		list7.Add("");
		array = StringFormat.ClipText(text, maxWidth).Split('\n');
		foreach (string item3 in array)
		{
			list7.Add(item3);
		}
		list7.Add("");
		Inscription = Event.NewStringBuilder().Append('ÿ').Append('ÿ')
			.Append('ÿ')
			.Append('É')
			.Append('Í', 31)
			.Append('»')
			.ToString();
		for (int j = 0; j < list7.Count; j++)
		{
			Inscription += "\nÿÿÿ";
			if (j % 2 == 0)
			{
				Inscription += "º";
			}
			else
			{
				Inscription += "¶";
			}
			int num2 = ColorUtility.LengthExceptFormatting(list7[j]);
			Inscription += list7[j].PadLeft(16 + num2 / 2, 'ÿ').PadRight(31, 'ÿ');
			if (j % 2 == 0)
			{
				Inscription += "º";
			}
			else
			{
				Inscription += "Ç";
			}
		}
		Inscription += Event.NewStringBuilder().Append('\n').Append('ÿ')
			.Append('ÿ')
			.Append('ÿ')
			.Append('Ó')
			.Append('Ä', 31)
			.Append('½')
			.ToString();
		NeedsGeneration = false;
	}
}
