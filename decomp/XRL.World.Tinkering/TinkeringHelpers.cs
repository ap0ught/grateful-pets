using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.World.Tinkering;

public static class TinkeringHelpers
{
	public static string TinkeredItemInventoryCategory(string Blueprint)
	{
		GameObject gameObject = GameObject.CreateSample(Blueprint);
		StripForTinkering(gameObject);
		ForceToBePowered(gameObject);
		if (gameObject.HasPart<Examiner>())
		{
			gameObject.GetPart<Examiner>().EpistemicStatus = 2;
		}
		string inventoryCategory = gameObject.GetInventoryCategory(AsIfKnown: true);
		gameObject.Obliterate();
		return inventoryCategory;
	}

	public static string TinkeredItemDisplayName(string Blueprint)
	{
		GameObject gameObject = GameObject.CreateSample(Blueprint);
		StripForTinkering(gameObject);
		ForceToBePowered(gameObject);
		string referenceDisplayName = gameObject.GetReferenceDisplayName(int.MaxValue, null, "Tinkering");
		gameObject.Obliterate();
		return referenceDisplayName;
	}

	public static string TinkeredItemShortDisplayName(string Blueprint)
	{
		GameObject gameObject = GameObject.CreateSample(Blueprint);
		StripForTinkering(gameObject);
		ForceToBePowered(gameObject);
		string referenceDisplayName = gameObject.GetReferenceDisplayName(int.MaxValue, null, "Tinkering", NoColor: false, Stripped: false, ColorOnly: false, WithoutTitles: false, Short: true);
		gameObject.Obliterate();
		return referenceDisplayName;
	}

	public static void ProcessTinkeredItem(GameObject Object, GameObject Actor)
	{
		StripForTinkering(Object);
		Object.MakeUnderstood();
		Object.SetIntProperty("TinkeredItem", 1);
		CheckMakersMark(Object, Actor, null, "Tinkering");
	}

	public static bool EligibleForMakersMark(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (Object.HasTag("AlwaysStack"))
		{
			return false;
		}
		if (!Object.HasPart<Description>())
		{
			return false;
		}
		if (ConsiderStandardScrap(Object))
		{
			return false;
		}
		return true;
	}

	public static void CheckMakersMark(GameObject Object, GameObject Actor, IModification ModAdded = null, string Context = null)
	{
		if (!EligibleForMakersMark(Object) || !GameObject.Validate(ref Actor))
		{
			return;
		}
		if (GlobalConfig.GetBoolSetting("DynamicMakersMarks") && !(Actor.IsPlayer() ? The.Game.GetBooleanGameState("PlayerMakersMarkDone") : Actor.HasPart<HasMakersMark>()) && TriggersMakersMarkCreationEvent.Check(Object, Actor, ModAdded, Context))
		{
			if (Actor.IsPlayer())
			{
				List<string> usable = MakersMark.GetUsable();
				usable.Insert(0, "none");
				char[] array = new char[usable.Count];
				char c = 'a';
				int i = 0;
				for (int count = usable.Count; i < count; i++)
				{
					array[i] = ((c <= 'z') ? c++ : ' ');
				}
				int num = Popup.PickOption("Select your maker's mark.", null, "", "Sounds/UI/ui_notification", usable.ToArray(), array);
				The.Game.SetBooleanGameState("PlayerMakersMarkDone", Value: true);
				if (num > 0)
				{
					string text = usable[num];
					The.Game.SetStringGameState("PlayerMakersMark", text);
					MakersMark.RecordUsage(text);
					string value = Popup.ShowColorPicker("Choose a color for your maker's mark.", 0, null, 60, RespectOptionNewlines: false, AllowEscape: false, "R", "", includeNone: false, includePatterns: false, allowBackground: false, text);
					The.Game.SetStringGameState("PlayerMakersMarkColor", value);
				}
			}
			else
			{
				HasMakersMark hasMakersMark = Actor.RequirePart<HasMakersMark>();
				hasMakersMark.Mark = MakersMark.Generate();
				hasMakersMark.Color = Crayons.GetRandomColor();
			}
		}
		HasMakersMark part = Actor.GetPart<HasMakersMark>();
		string text2 = part?.Mark;
		string text3 = part?.Color;
		if (Actor.IsPlayer())
		{
			text2 = The.Game.GetStringGameState("PlayerMakersMark", text2);
			text3 = The.Game.GetStringGameState("PlayerMakersMarkColor", text3);
		}
		if (!text2.IsNullOrEmpty())
		{
			Object.RequirePart<MakersMark>().AddCrafter(Actor, text2, text3 ?? "R");
		}
	}

	public static void StripForTinkering(GameObject Object)
	{
		if (Object.TryGetPart<EnergyCellSocket>(out var Part) && Part.Cell != null)
		{
			GameObject cell = Part.Cell;
			CellChangedEvent.Send(null, Object, cell, null);
			Part.Cell = null;
			cell.Obliterate();
		}
		if (Object.TryGetPart<MagazineAmmoLoader>(out var Part2))
		{
			Part2.Ammo = null;
		}
		Object.ForeachPartDescendedFrom((IEnergyCell P) =>
		{
			P.TinkerInitialize();
		});
		Object.LiquidVolume?.Empty();
	}

	public static void ForceToBePowered(GameObject Object)
	{
		int num = QueryDrawEvent.GetFor(Object) * 2;
		if (num < 1000)
		{
			num = 1000;
		}
		FreePower freePower = Object.RequirePart<FreePower>();
		if (freePower.ChargeFulfilled < num)
		{
			freePower.ChargeFulfilled = num;
		}
		Object.GetPart<Gaslight>()?.SyncActiveState();
	}

	public static bool ConsiderStandardScrap(GameObject Object)
	{
		return Object.HasTagOrProperty("Scrap");
	}

	public static bool ConsiderScrap(GameObject Object, GameObject Actor = null)
	{
		if (ConsiderStandardScrap(Object))
		{
			return true;
		}
		if (Actor != null)
		{
			return Tinkering_Disassemble.CheckScrapToggle(Object);
		}
		return false;
	}

	public static bool CanBeDisassembled(GameObject Object, GameObject Actor = null)
	{
		if (Object.TryGetPart<TinkerItem>(out var Part))
		{
			return Part.CanBeDisassembled(Actor);
		}
		return false;
	}
}
