using System.Collections.Generic;
using System.Linq;
using Qud.UI;
using UnityEngine;
using XRL;
using XRL.CharacterBuilds;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World.Parts;

namespace JoppaTutorial;

public class IntroTutorialStart : TutorialStep
{
	public override bool AllowCommand(string id)
	{
		return true;
	}

	public override bool AllowOnSelected(FrameworkDataElement element)
	{
		if (element.Id == "Name")
		{
			return true;
		}
		if (element.Id == "Chargen:Back")
		{
			return true;
		}
		if (element.Id == "Chargen:Next")
		{
			return true;
		}
		if (element.Id == "Mutated Human")
		{
			return true;
		}
		if (element.Id == "Marsh Taur")
		{
			return true;
		}
		if (element.Id == "JoppaTutorial")
		{
			return true;
		}
		return false;
	}

	public override void LateUpdate()
	{
		RectTransform rectTransform = null;
		string text = null;
		int num = 64;
		string directionHint = "nw";
		rectTransform = ControlId.Get("Mutated Human")?.gameObject?.transform as RectTransform;
		if (rectTransform != null)
		{
			text = "Welcome to the Caves of Qud tutorial. We'll just be scratching the surface here, learning enough of the basics to help you get your footing.\n\nIn Caves of Qud, you play as a mutated human or true kin.\n\nFor the tutorial, we're picking mutated human.";
			if (Media.sizeClass <= Media.SizeClass.Small)
			{
				directionHint = "ne";
			}
		}
		if (rectTransform == null)
		{
			rectTransform = ControlId.Get("Marsh Taur")?.gameObject?.transform as RectTransform;
			if (rectTransform != null)
			{
				text = "Character creation is a deep and sometimes long process. We included some preset builds to help you get started. After the tutorial, you can try another build, or make a character from scratch. (The recommended way! Once you get your footing.) \n\nFor now, pick the marsh taur.";
				if (Media.sizeClass <= Media.SizeClass.Small)
				{
					directionHint = "ne";
				}
			}
		}
		if (rectTransform == null)
		{
			rectTransform = ControlId.Get("JoppaTutorial")?.gameObject?.transform as RectTransform;
			text = "<noframe>";
		}
		if (rectTransform == null)
		{
			rectTransform = ControlId.Get("Chargen:Next")?.gameObject?.transform as RectTransform;
			if (UIManager.instance?.currentWindow != null)
			{
				if (UIManager.instance?.currentWindow?.name == "Summary")
				{
					text = "Here is a summary of your attributes and mutations, which grant your character unique abilities.";
					num = 16;
					if (Media.sizeClass <= Media.SizeClass.Small)
					{
						directionHint = "se";
					}
				}
				if (UIManager.instance?.currentWindow?.name == "Customize")
				{
					text = "You can name your character or choose Next for a random name.";
					num = 16;
					if (Media.sizeClass <= Media.SizeClass.Small)
					{
						directionHint = "se";
					}
				}
			}
			if (rectTransform == null)
			{
				rectTransform = ControlId.Get("Chargen:Next:Small")?.gameObject?.transform as RectTransform;
				text = "<noframe>";
			}
		}
		manager.Highlight(rectTransform?.gameObject?.transform as RectTransform, text, directionHint, num, num);
	}

	public override void OnBootGame(XRLGame game, EmbarkInfo info)
	{
		List<string> list = ActivatedAbilities.PreferenceOrder?.ToList() ?? new List<string>();
		if (list.Contains("CommandToggleRunning"))
		{
			list.Remove("CommandToggleRunning");
		}
		if (list.Contains("CommandFreezingRay"))
		{
			list.Remove("CommandFreezingRay");
		}
		if (list.Contains("CommandSurvivalCamp"))
		{
			list.Remove("CommandSurvivalCamp");
		}
		game.SetStringGameState("embarkIntroText", "On the 30th of Kisu Ux, you scuttle down a dark shaft at the edge of a sunken trade path and arrive at a caravanserai. It's powdered in salt and dust from across the ribbon of time.");
		game.SetStringGameState("JoppaWorldTutorial", "Yes");
		list.Insert(0, "CommandSurvivalCamp");
		list.Insert(0, "CommandFreezingRay");
		list.Insert(0, "CommandToggleRunning");
		ActivatedAbilities.PreferenceOrder = list;
		TutorialManager.AdvanceStep(new MoveToChest());
	}
}
