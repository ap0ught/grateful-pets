using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using Qud.API;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.Language;
using XRL.UI;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace Qud.UI;

[UIView("CharacterStatusScreen", false, false, false, null, null, false, 0, false, NavCategory = "Menus", UICanvas = "StatusScreens", UICanvasHost = 1)]
public class CharacterStatusScreen : BaseStatusScreen<CharacterStatusScreen>
{
	private class PsychicGlimmerProxyBaseMutation : BaseMutation
	{
		public int glimmer;

		private IRenderable renderable;

		public override IRenderable GetIcon()
		{
			if (renderable == null)
			{
				renderable = new Renderable("Tiles2/psychic-glimmer.png", " ", "&M", null, 'b');
			}
			return renderable;
		}

		public override string GetDescription()
		{
			return PsychicHunterSystem.GetPsychicGlimmerDescription(Level);
		}

		public override string GetLevelText(int Level)
		{
			return "";
		}

		public override int GetUIDisplayLevel()
		{
			return glimmer;
		}
	}

	public ScrollContext<NavigationContext> attributesNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> vertNav = new ScrollContext<NavigationContext>();

	public ScrollContext<NavigationContext> horizNav = new ScrollContext<NavigationContext>();

	public FrameworkScroller primaryAttributesController;

	public FrameworkScroller secondaryAttributesController;

	public FrameworkScroller resistanceAttributesController;

	public FrameworkScroller mutationsController;

	public FrameworkScroller effectsController;

	public UnityEngine.GameObject primaryAttributesHeader;

	public UnityEngine.GameObject secondaryAttributesHeader;

	public UnityEngine.GameObject resistanceAttributesHeader;

	public UITextSkin primaryAttributesHeaderText;

	public VerticalLayoutGroup attributesTopLayoutGroup;

	public UITextSkin primaryAttributesDetails;

	public UITextSkin secondaryAttributesDetails;

	public UITextSkin resistanceAttributesDetails;

	public UITextSkin mutationsDetails;

	public UITextSkin nameText;

	public UITextSkin classText;

	public UITextSkin levelText;

	public UIThreeColorProperties playerIcon;

	public UITextSkin attributePointsText;

	public UITextSkin mutationPointsText;

	private static List<Effect> effects = new List<Effect>(64);

	private static List<BaseMutation> mutations = new List<BaseMutation>(64);

	private static List<Statistic> stats = new List<Statistic>(64);

	private PsychicGlimmerProxyBaseMutation glimmerProxy = new PsychicGlimmerProxyBaseMutation();

	public UnityEngine.GameObject mutationDetailsObject;

	public Canvas mutationsPanelCanvas;

	private static string[] PrimaryAttributes = new string[6] { "Strength", "Agility", "Toughness", "Intelligence", "Willpower", "Ego" };

	private static string[] SecondaryAttributes = new string[5] { "Speed", "MoveSpeed", "AV", "DV", "MA" };

	private static string[] SecondaryAttributesWithCP = new string[6] { "Speed", "MoveSpeed", "AV", "DV", "MA", "CP" };

	private static string[] ResistanceAttributes = new string[4] { "AcidResistance", "ElectricResistance", "ColdResistance", "HeatResistance" };

	public string mutationsTerm;

	public string mutationTerm;

	public string mutationTermCapital;

	public string mutationColor;

	public static int CP = int.MinValue;

	public UITextSkin mutationTermText;

	public UIThreeColorProperties mutationIcon;

	public UITextSkin mutationTypeText;

	public UITextSkin mutationNameText;

	public UITextSkin mutationRankText;

	public static readonly MenuOption BUY_MUTATION = new MenuOption
	{
		Id = "BUY_MUTATION",
		KeyDescription = "Buy Mutation",
		InputCommand = "CmdStatusBuyMutation",
		Description = "Buy Mutation"
	};

	public static readonly MenuOption SHOW_EFFECTS = new MenuOption
	{
		Id = "SHOW_EFFECTS",
		KeyDescription = "Show Effects",
		InputCommand = "CmdStatusShowEffects",
		Description = "Show Effects"
	};

	private XRL.World.GameObject GO;

	public override string GetNavigationCategory()
	{
		return "StatusScreens:Charsheet";
	}

	public override IRenderable GetTabIcon()
	{
		return The.Player?.RenderForUI("StatusScreen,Tab");
	}

	public override string GetTabString()
	{
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			return "Attributes";
		}
		return "Attributes && Powers";
	}

	public void UpdateData()
	{
		mutations.Clear();
		stats.Clear();
		effects.Clear();
		if (GO != null)
		{
			effects.AddRange(GO.Effects.Where((Effect e) => e != null && !string.IsNullOrEmpty(e.GetDescription())));
			mutations.AddRange(StatusScreen.GetMutationList(StatusScreensScreen.GO));
			int psychicGlimmer = GO.GetPsychicGlimmer();
			if (PsychicGlimmer.Perceptible(psychicGlimmer))
			{
				glimmerProxy.glimmer = psychicGlimmer;
				glimmerProxy.SetName("Psychic Glimmer");
				glimmerProxy.SetDisplayName("Psychic Glimmer");
				mutations.Insert(0, glimmerProxy);
			}
			stats.AddRange(StatusScreensScreen.GO.Statistics.Values);
		}
		else
		{
			stats.Add(new Statistic("Strength", 0, 9000, 16, null));
			stats.Add(new Statistic("Agility", 0, 9000, 16, null));
			stats.Add(new Statistic("Toughness", 0, 9000, 16, null));
			stats.Add(new Statistic("Intelligence", 0, 9000, 16, null));
			stats.Add(new Statistic("Willpower", 0, 9000, 16, null));
			stats.Add(new Statistic("Ego", 0, 9000, 16, null));
		}
	}

	public override void UpdateViewFromData()
	{
		CP = GetAvailableComputePowerEvent.GetFor(GO);
		GetMutationTermEvent.GetFor(GO, out mutationTerm, out mutationColor);
		mutationsTerm = Grammar.MakeTitleCase(Grammar.Pluralize(mutationTerm));
		mutationTermCapital = Grammar.MakeTitleCase(mutationTerm);
		mutationTermText?.SetText(ConsoleLib.Console.ColorUtility.ToUpperExceptFormatting(mutationsTerm));
		primaryAttributesController.BeforeShow(from a in PrimaryAttributes
			where stats.Any((Statistic s) => s.Name == a)
			select new CharacterAttributeLineData
			{
				category = CharacterAttributeLineData.Category.primary,
				go = GO,
				data = stats.Where((Statistic s) => s.Name == a).FirstOrDefault()
			});
		secondaryAttributesController.BeforeShow(from a in (CP > 0) ? SecondaryAttributesWithCP : SecondaryAttributes
			where stats.Any((Statistic s) => s.Name == a) || a == "CP"
			select new CharacterAttributeLineData
			{
				category = CharacterAttributeLineData.Category.secondary,
				go = GO,
				data = stats.Where((Statistic s) => s.Name == a).FirstOrDefault(),
				stat = a
			});
		resistanceAttributesController.BeforeShow(from a in ResistanceAttributes
			where stats.Any((Statistic s) => s.Name == a)
			select new CharacterAttributeLineData
			{
				category = CharacterAttributeLineData.Category.resistance,
				go = GO,
				data = stats.Where((Statistic s) => s.Name == a).FirstOrDefault()
			});
		mutationsController.BeforeShow(mutations.Select((BaseMutation m) => new CharacterMutationLineData
		{
			mutation = m
		}));
		effectsController.BeforeShow(effects.Select((Effect e) => new CharacterEffectLineData
		{
			effect = e
		}));
		if (GO != null)
		{
			playerIcon.FromRenderable(GO.RenderForUI("StatusScreen,Character"));
			nameText.SetText(GO.DisplayName);
			classText.SetText(GO.GetGenotype() + " " + GO.GetSubtype());
			levelText.SetText(string.Format("Level: {0} \u00af HP: {1}/{2} \u00af XP: {3}/{4} \u00af Weight: {5}#", GO.Level, GO.Stat("Hitpoints"), GO.GetStat("Hitpoints").BaseValue, GO.Stat("XP"), Leveler.GetXPForLevel(GO.Stat("Level") + 1), GO.Weight));
			attributePointsText.SetText(string.Format("Attribute Points: {0}{1}}}}}", (GO.Stat("AP") > 0) ? "{{G|" : "{{K|", GO.Stat("AP")));
			if (Media.sizeClass < Media.SizeClass.Medium)
			{
				mutationPointsText.SetText(string.Format("MP: {0}{1}}}}}", (GO.Stat("MP") > 0) ? "{{G|" : "{{K|", GO.Stat("MP")));
			}
			else
			{
				mutationPointsText.SetText(string.Format("{0} Points: {1}{2}}}}}", mutationTermCapital, (GO.Stat("MP") > 0) ? "{{G|" : "{{K|", GO.Stat("MP")));
			}
		}
	}

	public void HandleHighlightMutation(FrameworkDataElement element)
	{
		if (element != null && element is CharacterMutationLineData { mutation: not null } characterMutationLineData)
		{
			mutationDetailsObject.SetActive(value: true);
			StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder(characterMutationLineData.mutation.GetDescription());
			bool num = StatusScreen.TreatAsMutant(GO) && characterMutationLineData.mutation.BaseLevel > 0;
			GetMutationTermEvent.GetFor(GO, characterMutationLineData.mutation);
			bool flag = !characterMutationLineData.mutation.CanLevel() || characterMutationLineData.mutation.BaseLevel >= characterMutationLineData.mutation.GetMaxLevel();
			mutationIcon.FromRenderable(characterMutationLineData.mutation.GetIcon());
			mutationNameText.SetText("{{B|" + characterMutationLineData.mutation.GetDisplayName() + "}}");
			if (!characterMutationLineData.mutation.ShouldShowLevel() || characterMutationLineData.mutation == glimmerProxy)
			{
				mutationRankText.SetText("");
			}
			else
			{
				mutationRankText.SetText($"{{{{G|RANK {characterMutationLineData.mutation.Level}/10}}}}");
			}
			if (characterMutationLineData.mutation == glimmerProxy)
			{
				mutationTypeText.SetText("");
			}
			else if (characterMutationLineData.mutation.Name == "Chimera" || characterMutationLineData.mutation.Name == "Esper" || characterMutationLineData.mutation.Name == "Unstable Genome")
			{
				mutationTypeText.SetText("{{C|[Morphotype]}}");
			}
			else if (characterMutationLineData.mutation.IsDefect())
			{
				mutationTypeText.SetText(characterMutationLineData.mutation.IsPhysical() ? "{{R|[Physical Defect]}}" : "{{r|[Mental Defect]}}");
			}
			else
			{
				mutationTypeText.SetText(characterMutationLineData.mutation.IsPhysical() ? ("{{c|[Physical " + mutationTermCapital + "]}}") : ("{{c|[Mental " + mutationTermCapital + "]}}"));
			}
			if ((!num | flag) || characterMutationLineData.mutation == glimmerProxy)
			{
				stringBuilder.Compound(characterMutationLineData.mutation.GetLevelText(characterMutationLineData.mutation.Level), "\n\n");
			}
			else
			{
				string value = characterMutationLineData.mutation.GetLevelText(characterMutationLineData.mutation.Level);
				string value2 = characterMutationLineData.mutation.GetLevelText(characterMutationLineData.mutation.Level + 1);
				if (!string.IsNullOrEmpty(value))
				{
					stringBuilder.Compound("{{w|This rank}}:\n", "\n\n").Append(characterMutationLineData.mutation.GetLevelText(characterMutationLineData.mutation.Level));
				}
				if (!string.IsNullOrEmpty(value2))
				{
					stringBuilder.Append("\n\n{{w|Next rank}}:\n").Append(characterMutationLineData.mutation.GetLevelText(characterMutationLineData.mutation.Level + 1));
				}
			}
			mutationsDetails.SetText(stringBuilder.ToString());
		}
		else
		{
			mutationsDetails.SetText("");
			mutationDetailsObject.SetActive(value: false);
		}
	}

	public void HandleHighlightEffect(FrameworkDataElement element)
	{
		if (element != null && element is CharacterEffectLineData { effect: not null } characterEffectLineData)
		{
			StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
			if (characterEffectLineData.effect.GetDescription() != null)
			{
				stringBuilder.AppendLine(Campfire.ProcessEffectDescription(characterEffectLineData.effect.GetDetails(), GO).Replace("\n", "\n  "));
				mutationsDetails.SetText(stringBuilder.ToString());
			}
		}
		else
		{
			mutationsDetails.SetText("");
		}
	}

	public async void HandleSelectPrimaryAttribute(FrameworkDataElement element)
	{
		CharacterAttributeLineData data = element as CharacterAttributeLineData;
		if (data != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				StatusScreen.BuyStat(GO, data.data.Name);
			});
			UpdateViewFromData();
		}
	}

	public async void HandleSelectMutation(FrameworkDataElement element)
	{
		CharacterMutationLineData data = element as CharacterMutationLineData;
		if (data != null)
		{
			await APIDispatch.RunAndWaitAsync(() =>
			{
				StatusScreen.ShowMutationPopup(GO, data.mutation);
			});
			UpdateData();
			UpdateViewFromData();
		}
	}

	public void HandleHighlightAttribute(FrameworkDataElement element)
	{
		if (!(element is CharacterAttributeLineData characterAttributeLineData))
		{
			return;
		}
		UITextSkin uITextSkin = null;
		if (Media.sizeClass > Media.SizeClass.Small)
		{
			if (characterAttributeLineData.category == CharacterAttributeLineData.Category.primary)
			{
				uITextSkin = primaryAttributesDetails;
			}
			if (characterAttributeLineData.category == CharacterAttributeLineData.Category.secondary)
			{
				uITextSkin = secondaryAttributesDetails;
			}
			if (characterAttributeLineData.category == CharacterAttributeLineData.Category.resistance)
			{
				uITextSkin = resistanceAttributesDetails;
			}
		}
		else
		{
			uITextSkin = resistanceAttributesDetails;
		}
		if (uITextSkin != primaryAttributesDetails)
		{
			primaryAttributesDetails.SetText("");
		}
		if (uITextSkin != secondaryAttributesDetails)
		{
			secondaryAttributesDetails.SetText("");
		}
		if (uITextSkin != resistanceAttributesDetails)
		{
			resistanceAttributesDetails.SetText("");
		}
		if (uITextSkin != null)
		{
			if (characterAttributeLineData.stat == "CP")
			{
				uITextSkin.SetText("Your {{W|Compute Power (CP)}} scales the bonuses of certain compute-enabled items and cybernetic implants. Your base compute power is 0.");
			}
			else
			{
				uITextSkin.SetText(characterAttributeLineData.data.GetHelpText());
			}
		}
	}

	public async void HandleBuyMutation()
	{
		await APIDispatch.RunAndWaitAsync(() =>
		{
			MutationsAPI.BuyRandomMutation(GO);
		});
		UpdateData();
		UpdateViewFromData();
	}

	public async void HandleShowEffects()
	{
		await APIDispatch.RunAndWaitAsync(() =>
		{
			The.Player.ShowActiveEffects();
		});
	}

	public override NavigationContext ShowScreen(XRL.World.GameObject GO, StatusScreensScreen parent)
	{
		this.GO = GO;
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			if (attributesTopLayoutGroup.spacing != -8f)
			{
				attributesTopLayoutGroup.spacing = -8f;
			}
			primaryAttributesHeaderText.SetText("ATTRIBUTES");
		}
		else
		{
			if (attributesTopLayoutGroup.spacing != 0f)
			{
				attributesTopLayoutGroup.spacing = 0f;
			}
			primaryAttributesHeaderText.SetText("MAIN ATTRIBUTES");
		}
		vertNav.contexts.Clear();
		vertNav.SetAxis(InputAxisTypes.NavigationYAxis);
		vertNav.contexts.Add(primaryAttributesController.scrollContext);
		vertNav.contexts.Add(secondaryAttributesController.scrollContext);
		vertNav.contexts.Add(resistanceAttributesController.scrollContext);
		vertNav.wraps = false;
		vertNav.Setup();
		horizNav.contexts.Clear();
		horizNav.SetAxis(InputAxisTypes.NavigationXAxis);
		horizNav.contexts.Add(vertNav);
		horizNav.contexts.Add(mutationsController.scrollContext);
		horizNav.Setup();
		primaryAttributesController.onSelected.RemoveAllListeners();
		primaryAttributesController.onHighlight.RemoveAllListeners();
		primaryAttributesController.onHighlight.AddListener(HandleHighlightAttribute);
		primaryAttributesController.onSelected.AddListener(HandleSelectPrimaryAttribute);
		primaryAttributesController.scrollContext.wraps = false;
		secondaryAttributesController.onSelected.RemoveAllListeners();
		secondaryAttributesController.onHighlight.RemoveAllListeners();
		secondaryAttributesController.onHighlight.AddListener(HandleHighlightAttribute);
		secondaryAttributesController.scrollContext.wraps = false;
		resistanceAttributesController.onSelected.RemoveAllListeners();
		resistanceAttributesController.onHighlight.RemoveAllListeners();
		resistanceAttributesController.onHighlight.AddListener(HandleHighlightAttribute);
		resistanceAttributesController.scrollContext.wraps = false;
		mutationsController.onSelected.RemoveAllListeners();
		mutationsController.onHighlight.RemoveAllListeners();
		mutationsController.onHighlight.AddListener(HandleHighlightMutation);
		mutationsController.onSelected.AddListener(HandleSelectMutation);
		effectsController.onSelected.RemoveAllListeners();
		effectsController.onHighlight.RemoveAllListeners();
		effectsController.onHighlight.AddListener(HandleHighlightEffect);
		UpdateData();
		UpdateViewFromData();
		primaryAttributesController.selectedPosition = 0;
		primaryAttributesController.scrollContext.ActivateAndEnable();
		HandleHighlightMutation(new CharacterMutationLineData
		{
			mutation = mutations.FirstOrDefault()
		});
		ScrollContext<NavigationContext> scrollContext = horizNav;
		if (scrollContext.commandHandlers == null)
		{
			scrollContext.commandHandlers = new Dictionary<string, Action>();
		}
		horizNav.commandHandlers.Clear();
		scrollContext = horizNav;
		if (scrollContext.menuOptionDescriptions == null)
		{
			List<MenuOption> list = (scrollContext.menuOptionDescriptions = new List<MenuOption>());
		}
		horizNav.menuOptionDescriptions.Clear();
		if (GO != null && GO.IsTrueKin() && mutations.Count == 0)
		{
			mutationsPanelCanvas.enabled = false;
			horizNav.commandHandlers.Add(SHOW_EFFECTS.InputCommand, XRL.UI.Framework.Event.Helpers.Handle(() =>
			{
				HandleShowEffects();
			}));
			horizNav.menuOptionDescriptions.Add(SHOW_EFFECTS);
		}
		else
		{
			mutationsPanelCanvas.enabled = true;
			horizNav.commandHandlers.Add(BUY_MUTATION.InputCommand, XRL.UI.Framework.Event.Helpers.Handle(() =>
			{
				HandleBuyMutation();
			}));
			horizNav.commandHandlers.Add(SHOW_EFFECTS.InputCommand, XRL.UI.Framework.Event.Helpers.Handle(() =>
			{
				HandleShowEffects();
			}));
			horizNav.menuOptionDescriptions.Add(BUY_MUTATION);
			horizNav.menuOptionDescriptions.Add(SHOW_EFFECTS);
		}
		NavigationContext screenGlobalContext = parent.screenGlobalContext;
		if (screenGlobalContext.menuOptionDescriptions == null)
		{
			List<MenuOption> list = (screenGlobalContext.menuOptionDescriptions = new List<MenuOption>());
		}
		parent.screenGlobalContext.menuOptionDescriptions.Add(SHOW_EFFECTS);
		parent.screenGlobalContext.menuOptionDescriptions.Add(BUY_MUTATION);
		screenGlobalContext = parent.screenGlobalContext;
		if (screenGlobalContext.commandHandlers == null)
		{
			screenGlobalContext.commandHandlers = new Dictionary<string, Action>();
		}
		parent.screenGlobalContext.commandHandlers["CmdStatusShowEffects"] = HandleShowEffects;
		parent.screenGlobalContext.commandHandlers["CmdStatusBuyMutation"] = HandleBuyMutation;
		base.ShowScreen(GO, parent);
		return horizNav;
	}

	public override void Prewarm()
	{
		UpdateViewFromData();
	}
}
