using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XRL.CharacterBuilds.Qud.UI;
using XRL.CharacterCreation;
using XRL.World;
using XRL.World.Parts;

namespace XRL.CharacterBuilds.Qud;

public class QudGenotypeModule : QudEmbarkBuilderModule<QudGenotypeModuleData>
{
	private List<GenotypeEntry> PregenGenotypes;

	public Dictionary<string, GenotypeEntry> genotypesByName => GenotypeFactory.GenotypesByName;

	public List<GenotypeEntry> genotypes
	{
		get
		{
			if (builder.GetModule<QudChartypeModule>()?.data?.type == "Pregen")
			{
				if (PregenGenotypes == null)
				{
					Dictionary<string, QudPregenModule.QudPregenData>.ValueCollection pregens = builder.GetModule<QudPregenModule>()?.pregens?.Values;
					PregenGenotypes = ((pregens != null) ? GenotypeFactory.Genotypes.Where((GenotypeEntry g) => pregens.Any((QudPregenModule.QudPregenData p) => p.Genotype == g.Name)).ToList() : GenotypeFactory.Genotypes);
				}
				return PregenGenotypes;
			}
			return GenotypeFactory.Genotypes;
		}
	}

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudChartypeModule>()?.data != null;
	}

	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public override void RandomSelectionNoUI()
	{
		setData(new QudGenotypeModuleData(genotypes.GetRandomElement().Name));
	}

	public void SelectGenotype(string id)
	{
		setData(new QudGenotypeModuleData(id));
	}

	public string getSelected()
	{
		return data?.Genotype;
	}

	public override void InitFromSeed(string seed)
	{
		BallBag<GenotypeEntry> ballBag = new BallBag<GenotypeEntry>(getModuleRngFromSeed(seed));
		foreach (GenotypeEntry genotype in genotypes)
		{
			ballBag.Add(genotype, genotype.RandomWeight);
		}
		QudGenotypeModuleData qudGenotypeModuleData = new QudGenotypeModuleData(ballBag.PeekOne().Name);
		setData(qudGenotypeModuleData);
	}

	public override object handleUIEvent(string ID, object Element = null)
	{
		GenotypeEntry genotypeEntry = data?.Entry;
		if (genotypeEntry == null)
		{
			return base.handleUIEvent(ID, Element);
		}
		if (ID == QudAttributesModuleWindow.EID_GET_BASE_AP && genotypeEntry.StatPoints != -999)
		{
			return genotypeEntry.StatPoints;
		}
		if (ID == QudMutationsModuleWindow.EID_GET_BASE_MP && genotypeEntry.MutationPoints != -999)
		{
			return genotypeEntry.MutationPoints;
		}
		if (ID == QudMutationsModuleWindow.EID_GET_CATEGORIES && genotypeEntry.AllowedMutationCategories != null)
		{
			return genotypeEntry.AllowedMutationCategoriesList;
		}
		if (ID == QudAttributesModuleWindow.EID_BEFORE_GET_BASE_ATTRIBUTES)
		{
			List<AttributeDataElement> list = Element as List<AttributeDataElement>;
			foreach (KeyValuePair<string, GenotypeStat> stat2 in genotypeEntry.Stats)
			{
				list?.Add(new AttributeDataElement
				{
					Attribute = stat2.Key,
					BaseValue = stat2.Value.Minimum,
					Minimum = stat2.Value.Minimum,
					Maximum = stat2.Value.Maximum,
					Description = stat2.Value.ChargenDescription,
					Purchased = 0
				});
			}
		}
		else if (ID == QudAttributesModuleWindow.EID_GET_BASE_ATTRIBUTES)
		{
			List<AttributeDataElement> list2 = Element as List<AttributeDataElement>;
			foreach (KeyValuePair<string, GenotypeStat> stat in genotypeEntry.Stats)
			{
				AttributeDataElement attributeDataElement = ((list2 != null) ? list2.FirstOrDefault((AttributeDataElement a) => a.Attribute == stat.Value.Name) : null);
				if (attributeDataElement == null)
				{
					MetricsManager.LogError("Attribute " + stat.Value.Name + " did not exist in base attributes of genotype " + genotypeEntry.Name);
				}
				else if (stat.Value.Bonus != -999)
				{
					attributeDataElement.AddBonus(stat.Value.Bonus, "{{important|" + data.Entry.DisplayName + "}} genotype");
				}
			}
		}
		return base.handleUIEvent(ID, Element);
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		GenotypeEntry entry = data.Entry;
		if (entry == null)
		{
			return base.handleBootEvent(id, game, info, element);
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
		{
			XRL.World.GameObject gameObject = (XRL.World.GameObject)element;
			gameObject.SetStringProperty("Genotype", entry.Name);
			List<string> exclude = info.getData<QudSubtypeModuleData>()?.Entry?.RemoveSkills;
			entry.AddSkills(gameObject, exclude);
			if (entry.CyberneticsLicensePoints != -999)
			{
				gameObject.ModIntProperty("CyberneticsLicenses", entry.CyberneticsLicensePoints);
			}
			if (entry.SaveModifiers.Any((GenotypeSaveModifier x) => x.Amount != 0 && x.Amount != -999))
			{
				SaveModifiers saveModifiers = gameObject.RequirePart<SaveModifiers>();
				saveModifiers.WorksOnSelf = true;
				saveModifiers.WorksOnEquipper = false;
				foreach (GenotypeSaveModifier saveModifier in entry.SaveModifiers)
				{
					if (saveModifier.Amount != 0 && saveModifier.Amount != -999)
					{
						saveModifiers.AddModifier(saveModifier.Vs, saveModifier.Amount);
					}
				}
			}
			foreach (GenotypeReputation reputation in entry.Reputations)
			{
				Faction.PlayerReputation.Modify(reputation.With, reputation.Value, "Genotype", null, null, Silent: true);
			}
		}
		else if (id == QudGameBootModule.BOOTEVENT_AFTERBOOTPLAYEROBJECT && !entry.Class.IsNullOrEmpty())
		{
			XRL.World.GameObject body = (XRL.World.GameObject)element;
			string[] array = entry.Class.Split(',');
			foreach (string text in array)
			{
				if (!text.IsNullOrEmpty())
				{
					try
					{
						(ModManager.CreateInstance("XRL.CharacterCreation." + text) as ICustomChargenClass).BuildCharacterBody(body);
					}
					catch (Exception ex)
					{
						Debug.LogError("Exception executing builder: " + text + " -> " + ex);
					}
				}
			}
		}
		return base.handleBootEvent(id, game, info, element);
	}
}
