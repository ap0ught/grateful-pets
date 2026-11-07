using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenInvokeHigherBeing : SifrahPrioritizableToken
{
	public Worshippable Being;

	public RitualSifrahTokenInvokeHigherBeing()
	{
		Description = "invoke a higher being";
		Tile = "Items/sw_chiral_rings.bmp";
		RenderString = "\u0015";
		ColorString = "&K";
		DetailColor = 'm';
	}

	public RitualSifrahTokenInvokeHigherBeing(Worshippable Being, List<Worshippable> Beings = null)
		: this()
	{
		SetBeing(Being, Beings);
	}

	public static string GenerateLabel(Worshippable Being, List<Worshippable> Beings = null, string Verb = null)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (!Verb.IsNullOrEmpty())
		{
			stringBuilder.Append(Verb).Append(' ');
		}
		stringBuilder.Append(Being.Name);
		if (Worshippable.Multiple(Beings, Being))
		{
			if (!Verb.IsNullOrEmpty())
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(" in the manner of ").Append(Faction.GetFormattedName(Being.Faction));
		}
		return stringBuilder.ToString();
	}

	public void SetBeing(Worshippable Being, List<Worshippable> Beings = null)
	{
		this.Being = Being;
		Description = GenerateLabel(Being, Beings, "invoke");
		string text = Being.Blueprints;
		if (text == null)
		{
			return;
		}
		if (text.Contains(";") && !text.Contains(";;"))
		{
			text = text.Replace(";", ";;");
		}
		foreach (string item in text.CachedDoubleSemicolonExpansion())
		{
			GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(item);
			if (blueprintIfExists == null)
			{
				continue;
			}
			string partParameter = blueprintIfExists.GetPartParameter("Render", "Tile", Tile);
			if (partParameter.IsNullOrEmpty())
			{
				continue;
			}
			Tile = partParameter;
			string tag = blueprintIfExists.GetTag("WorshipColorPool");
			if (!tag.IsNullOrEmpty())
			{
				string text2 = "WorshipColorPool/" + tag + "/ColorString/" + Being.Name;
				string text3 = "WorshipColorPool/" + tag + "/DetailColor/" + Being.Name;
				if (The.Game.HasStringGameState(text2) && The.Game.HasStringGameState(text3))
				{
					ColorString = The.Game.GetStringGameState(text2);
					DetailColor = The.Game.GetStringGameState(text3)[0];
				}
				else
				{
					string state = "WorshipColorPool/" + tag + "/UsedColorSets";
					string stringGameState = The.Game.GetStringGameState(state);
					int num = 0;
					string randomColor;
					string randomColor2;
					string substring;
					do
					{
						randomColor = Crayons.GetRandomColor();
						randomColor2 = Crayons.GetRandomColor();
						substring = randomColor + randomColor2;
					}
					while ((randomColor == randomColor2 && ++num < 100) || (stringGameState.HasDelimitedSubstring(',', substring) && ++num < 20));
					The.Game.SetStringGameState(state, stringGameState.AddDelimitedSubstring(',', substring));
					ColorString = "&" + randomColor;
					DetailColor = randomColor2[0];
					The.Game.SetStringGameState(text2, ColorString);
					The.Game.SetStringGameState(text3, randomColor2);
				}
			}
			string tag2 = blueprintIfExists.GetTag("WorshipColorString");
			if (!tag2.IsNullOrEmpty())
			{
				ColorString = tag2;
			}
			string tag3 = blueprintIfExists.GetTag("WorshipDetailColor");
			if (!tag3.IsNullOrEmpty())
			{
				DetailColor = tag3[0];
			}
			break;
		}
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			return 0;
		}
		if (Being == null)
		{
			return Math.Max(Factions.GetWorshippables().Count - 1, 0);
		}
		if (!The.Game.PlayerReputation.HasWorshipped(Being.Faction))
		{
			int worshipValence = The.Game.PlayerReputation.GetWorshipValence(Being.Faction);
			if (worshipValence < 0)
			{
				return Math.Max(10 + worshipValence / 10, 0) + (Being.Name?.Length ?? 1);
			}
		}
		return 1073741823 + Being.Power + (Being.Name?.Length ?? 0);
	}

	public override int GetTiebreakerPriority()
	{
		if (Being == null)
		{
			return 7;
		}
		return 20 + Being.Power + ((!Being.Name.IsNullOrEmpty()) ? (Being.Name[0] - 65) : 0);
	}

	public bool IsAvailable()
	{
		if (Being != null)
		{
			return !The.Game.PlayerReputation.HasBlasphemed(Being, 36000);
		}
		return true;
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (Being == null)
		{
			List<Worshippable> worshippables = Factions.GetWorshippables();
			if (!worshippables.IsNullOrEmpty())
			{
				string[] array = new string[worshippables.Count];
				char[] array2 = new char[worshippables.Count];
				char c = 'a';
				int num = 0;
				foreach (Worshippable item in worshippables)
				{
					array[num] = GenerateLabel(item, worshippables);
					array2[num] = ((c > 'z') ? ' ' : c);
					if (c <= 'z')
					{
						c = (char)(c + 1);
					}
					num++;
				}
				int num2 = Popup.PickOption("Invoke whom?", null, "", "Sounds/UI/ui_notification", array, array2, null, null, null, null, null, 2, 60, 0, -1, AllowEscape: true);
				if (num2 < 0)
				{
					return false;
				}
				SetBeing(worshippables[num2]);
			}
		}
		if (!IsAvailable())
		{
			Popup.ShowFail("You have blasphemed against " + Being.Name + ".");
			return false;
		}
		return true;
	}

	public override int GetPowerup(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (Slot.CurrentMove == Slot.Token && Being != null && Being.Power >= 50)
		{
			return 1;
		}
		return base.GetPowerup(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		The.Game.PlayerReputation.WorshipPerformed(Being);
	}
}
