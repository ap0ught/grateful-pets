using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering : BaseSkill
{
	public static readonly string LEARN_NEW_RECIPE_CONTEXT = "LearnNewRecipe";

	[NonSerialized]
	public static bool IsPlayerOverride = false;

	public static void LearnNewRecipe(GameObject Actor, int MinTier, int MaxTier)
	{
		int num = ((!Actor.IsPlayer()) ? 1 : 3);
		List<GameObject> list = new List<GameObject>(num);
		List<TinkerData> list2 = new List<TinkerData>(num);
		while (list.Count < num)
		{
			GameObject gameObject = GameObject.Create("DataDisk", 0, 0, null, null, null, LEARN_NEW_RECIPE_CONTEXT);
			DataDisk part = gameObject.GetPart<DataDisk>();
			if (part != null && part.Data.Tier >= MinTier && part.Data.Tier <= MaxTier && !list2.CleanContains(part.Data))
			{
				list.Add(gameObject);
				list2.Add(part.Data);
			}
			else
			{
				gameObject.Obliterate();
			}
		}
		GameObject gameObject2 = null;
		if ((IsPlayerOverride || Actor.IsPlayer()) && !Popup.Suppress)
		{
			string[] array = new string[num];
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				array[i] = list[i].DisplayName;
			}
			int num2;
			for (num2 = -1; num2 < 0; num2 = Popup.PickOption("", "Choose a schematic.", "", "Sounds/UI/ui_notification", array))
			{
			}
			gameObject2 = list[num2];
		}
		else
		{
			gameObject2 = list[0];
		}
		TinkeringHelpers.CheckMakersMark(gameObject2, Actor);
		Actor.ReceiveObject(gameObject2, NoStack: false, LEARN_NEW_RECIPE_CONTEXT);
		if (Actor.IsPlayer())
		{
			Popup.Show("You have a flash of insight and scribe " + gameObject2.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false) + ".", null, "sfx_characterMod_tinkerSchematic_learn");
		}
	}

	public static int GetIdentifyLevel(GameObject who)
	{
		if (who == null)
		{
			return 0;
		}
		int num = 0;
		if (who.HasSkill("Tinkering_GadgetInspector") && !who.HasPart<Dystechnia>())
		{
			num += 3;
			if (who.HasSkill("Tinkering_Tinker2"))
			{
				num += 2;
			}
			if (who.HasSkill("Tinkering_Tinker3"))
			{
				num++;
			}
			int bonus = num * (who.StatMod("Intelligence") * 3) / 100;
			num += GetTinkeringBonusEvent.GetFor(who, null, "Inspect", num, bonus);
		}
		return num;
	}
}
