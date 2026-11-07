using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using XRL.CharacterBuilds.Qud.UI;
using XRL.CharacterCreation;
using XRL.UI.Framework;
using XRL.World;
using XRL.World.Parts;

namespace XRL.CharacterBuilds.Qud;

public class QudSubtypeModule : QudEmbarkBuilderModule<QudSubtypeModuleData>
{
	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILE = "BootPlayerTile";

	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILEFOREGROUND = "BootPlayerTileForeground";

	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILEBACKGROUND = "BootPlayerTileBackground";

	[Obsolete]
	public const string BOOTEVENT_BOOTPLAYERTILEDETAIL = "BootPlayerTileDetail";

	public Dictionary<string, SubtypeEntry> genotypesByName => SubtypeFactory.SubtypesByName;

	public List<SubtypeEntry> subtypes => SubtypeFactory.Subtypes;

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudGenotypeModule>()?.data?.Genotype != null;
	}

	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public void SelectSubtype(string id)
	{
		setData(new QudSubtypeModuleData(id));
	}

	public override void handleModuleDataChange(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		if (module is QudGenotypeModule && data != null)
		{
			if (builder.GetModule<QudGenotypeModule>().data?.Genotype == null)
			{
				setData(null);
			}
			else if (!GetSelections().Any((ChoiceWithColorIcon s) => s.Id == data.Subtype))
			{
				setData(null);
			}
		}
		base.handleModuleDataChange(module, oldValues, newValues);
	}

	public override void InitFromSeed(string seed)
	{
		BallBag<SubtypeEntry> ballBag = new BallBag<SubtypeEntry>(getModuleRngFromSeed(seed));
		foreach (SubtypeEntry subtype in SubtypeFactory.Subtypes)
		{
			ballBag.Add(subtype, subtype.RandomWeight);
		}
		QudSubtypeModuleData qudSubtypeModuleData = new QudSubtypeModuleData();
		qudSubtypeModuleData.Subtype = ballBag.PeekOne().Name;
		data = qudSubtypeModuleData;
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		SubtypeEntry subtypeEntry = data?.Entry;
		if (subtypeEntry == null)
		{
			return base.handleBootEvent(id, game, info, element);
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILE)
		{
			return element ?? subtypeEntry.Tile;
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERTILEDETAIL)
		{
			return element ?? subtypeEntry.DetailColor;
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
		{
			XRL.World.GameObject gameObject = (XRL.World.GameObject)element;
			gameObject.SetStringProperty("Subtype", data.Subtype);
			if (!subtypeEntry.Gear.IsNullOrEmpty())
			{
				string[] array = subtypeEntry.Gear.Split(',');
				foreach (string text in array)
				{
					if (PopulationManager.Populations.ContainsKey(text))
					{
						foreach (PopulationResult item in PopulationManager.Populations[text].Generate(new Dictionary<string, string>(), ""))
						{
							AbstractQudEmbarkBuilderModule.AddItem(item.Blueprint, item.Number, gameObject);
						}
					}
					else
					{
						Debug.LogError("Unknown gear population table: " + text);
					}
				}
				gameObject.CheckStacks();
			}
			List<string> exclude = info.getData<QudGenotypeModuleData>()?.Entry?.RemoveSkills;
			subtypeEntry.AddSkills(gameObject, exclude);
			if (subtypeEntry.CyberneticsLicensePoints != -999)
			{
				gameObject.ModIntProperty("CyberneticsLicenses", subtypeEntry.CyberneticsLicensePoints);
			}
			if (subtypeEntry.SaveModifiers.Any((SubtypeSaveModifier x) => x.Amount != 0 && x.Amount != -999))
			{
				SaveModifiers saveModifiers = gameObject.RequirePart<SaveModifiers>();
				saveModifiers.WorksOnSelf = true;
				saveModifiers.WorksOnEquipper = false;
				foreach (SubtypeSaveModifier saveModifier in subtypeEntry.SaveModifiers)
				{
					if (saveModifier.Amount != 0 && saveModifier.Amount != -999)
					{
						saveModifiers.AddModifier(saveModifier.Vs, saveModifier.Amount);
					}
				}
			}
		}
		else if (id == QudGameBootModule.BOOTEVENT_AFTERBOOTPLAYEROBJECT && !subtypeEntry.Class.IsNullOrEmpty())
		{
			XRL.World.GameObject body = (XRL.World.GameObject)element;
			string[] array = subtypeEntry.Class.Split(',');
			foreach (string text2 in array)
			{
				if (!text2.IsNullOrEmpty())
				{
					try
					{
						(ModManager.CreateInstance("XRL.CharacterCreation." + text2) as ICustomChargenClass).BuildCharacterBody(body);
					}
					catch (Exception ex)
					{
						Debug.LogError("Exception executing builder: " + text2 + " -> " + ex);
					}
				}
			}
		}
		else if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYERREPUTATION)
		{
			foreach (SubtypeReputation reputation in subtypeEntry.Reputations)
			{
				Faction.PlayerReputation.Modify(reputation.With, reputation.Value, "Subtype", null, null, Silent: true);
			}
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public override object handleUIEvent(string ID, object Element)
	{
		SubtypeEntry subtypeEntry = data?.Entry;
		if (subtypeEntry == null)
		{
			return base.handleUIEvent(ID, Element);
		}
		if (ID == QudAttributesModuleWindow.EID_BEFORE_GET_BASE_ATTRIBUTES)
		{
			List<AttributeDataElement> list = Element as List<AttributeDataElement>;
			foreach (KeyValuePair<string, SubtypeStat> stat in subtypeEntry.Stats)
			{
				AttributeDataElement attributeDataElement = ((list != null) ? list.FirstOrDefault((AttributeDataElement a) => a.Attribute == stat.Value.Name) : null);
				if (attributeDataElement != null)
				{
					if (stat.Value.Minimum != -999)
					{
						attributeDataElement.BaseValue = stat.Value.Minimum;
					}
					if (stat.Value.Minimum != -999)
					{
						attributeDataElement.Minimum = stat.Value.Minimum;
					}
					if (stat.Value.Maximum != -999)
					{
						attributeDataElement.Maximum = stat.Value.Maximum;
					}
				}
			}
		}
		else if (ID == QudAttributesModuleWindow.EID_GET_BASE_ATTRIBUTES)
		{
			List<AttributeDataElement> source = Element as List<AttributeDataElement>;
			foreach (KeyValuePair<string, SubtypeStat> stat2 in subtypeEntry.Stats)
			{
				AttributeDataElement attributeDataElement2 = source.FirstOrDefault((AttributeDataElement a) => a.Attribute == stat2.Value.Name);
				if (attributeDataElement2 != null && stat2.Value.Bonus != -999)
				{
					attributeDataElement2.AddBonus(stat2.Value.Bonus, "{{important|" + data.Entry.DisplayName + "}} " + SubtypeSingularName());
				}
			}
		}
		return base.handleUIEvent(ID, Element);
	}

	public string getSelected()
	{
		return data?.Subtype;
	}

	public bool hasCategories()
	{
		if (TryGetSubtypeClass(out var Subtypes))
		{
			return Subtypes.Categories.Count > 1;
		}
		return false;
	}

	public IEnumerable<SubtypeEntry> getAvailableSelections()
	{
		if (TryGetSubtypeClass(out var Subtypes))
		{
			return Subtypes.Categories.SelectMany((SubtypeCategory c) => c.Subtypes);
		}
		return new SubtypeEntry[0];
	}

	public ChoiceWithColorIcon choiceFromSubtype(SubtypeEntry subtype)
	{
		return new ChoiceWithColorIcon
		{
			Id = subtype.Name,
			Title = subtype.DisplayName,
			IconPath = subtype.Tile,
			HFlip = true,
			IconDetailColor = ConsoleLib.Console.ColorUtility.ColorFromString(subtype.DetailColor),
			IconForegroundColor = The.Color.Gray,
			Description = subtype.GetFlatChargenInfo(),
			Chosen = IsChoiceSelected
		};
	}

	public string SubtypeSingularName()
	{
		if (!TryGetSubtypeClass(out var Subtypes))
		{
			return "subtype";
		}
		return Subtypes.SingluarTitle;
	}

	public bool TryGetSubtypeClass(out SubtypeClass Subtypes)
	{
		return SubtypeFactory.TryGetSubtypeClass(builder.GetModule<QudGenotypeModule>()?.data?.Entry, out Subtypes);
	}

	public IEnumerable<ChoiceWithColorIcon> GetSelections()
	{
		foreach (SubtypeEntry availableSelection in getAvailableSelections())
		{
			yield return choiceFromSubtype(availableSelection);
		}
	}

	public IEnumerable<CategoryIcons> GetSelectionCategories()
	{
		if (!TryGetSubtypeClass(out var Subtypes))
		{
			yield break;
		}
		foreach (SubtypeCategory category in Subtypes.Categories)
		{
			if (category.Subtypes.Count > 0)
			{
				yield return new CategoryIcons
				{
					Title = category.DisplayName,
					Choices = category.Subtypes.Select(choiceFromSubtype).ToList()
				};
			}
		}
	}

	public bool IsChoiceSelected(ChoiceWithColorIcon choice)
	{
		return getSelected() == choice?.Id;
	}
}
