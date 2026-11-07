using System.Collections.Generic;
using System.Text;
using XRL.CharacterBuilds.Qud.UI;
using XRL.UI.Framework;
using XRL.World;

namespace XRL.CharacterBuilds.Qud;

public class QudAttributesModule : QudEmbarkBuilderModule<QudAttributesModuleData>
{
	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
	}

	public override bool shouldBeEditable()
	{
		return builder.IsEditableGameMode();
	}

	public override string DataWarnings()
	{
		_ = (int)builder.handleUIEvent(QudAttributesModuleWindow.EID_GET_BASE_AP, 0);
		QudAttributesModuleData qudAttributesModuleData = data;
		if (qudAttributesModuleData != null)
		{
			_ = qudAttributesModuleData.apRemaining;
			if (0 == 0)
			{
				if (data.apRemaining > 0)
				{
					return "You have unspent attribute points.";
				}
				return base.DataWarnings();
			}
		}
		return "You have unspent attribute points.";
	}

	public override string DataErrors()
	{
		_ = (int)builder.handleUIEvent(QudAttributesModuleWindow.EID_GET_BASE_AP, 0);
		QudAttributesModuleData qudAttributesModuleData = data;
		if (qudAttributesModuleData != null)
		{
			_ = qudAttributesModuleData.apRemaining;
			if (0 == 0)
			{
				if (data.apRemaining < 0)
				{
					return "You have spent too many attribute points!";
				}
				return base.DataErrors();
			}
		}
		return base.DataErrors();
	}

	public override void InitFromSeed(string seed)
	{
	}

	public Dictionary<string, int> GetTypeStats(GenotypeEntry GenotypeEntry, SubtypeEntry SubtypeEntry)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (GenotypeEntry != null)
		{
			foreach (KeyValuePair<string, GenotypeStat> stat in GenotypeEntry.Stats)
			{
				if (stat.Value.Minimum == -999)
				{
					dictionary.TryAdd(stat.Key, 0);
				}
				else
				{
					dictionary[stat.Key] = stat.Value.Minimum;
				}
			}
		}
		if (SubtypeEntry != null)
		{
			foreach (KeyValuePair<string, SubtypeStat> stat2 in SubtypeEntry.Stats)
			{
				if (stat2.Value.Minimum == -999)
				{
					dictionary.TryAdd(stat2.Key, 0);
				}
				else
				{
					dictionary[stat2.Key] = stat2.Value.Minimum;
				}
			}
		}
		if (GenotypeEntry != null)
		{
			foreach (KeyValuePair<string, GenotypeStat> stat3 in GenotypeEntry.Stats)
			{
				dictionary.TryGetValue(stat3.Key, out var value);
				dictionary[stat3.Key] = value + ((stat3.Value.Bonus != -999) ? stat3.Value.Bonus : 0);
			}
		}
		if (SubtypeEntry != null)
		{
			foreach (KeyValuePair<string, SubtypeStat> stat4 in SubtypeEntry.Stats)
			{
				dictionary.TryGetValue(stat4.Key, out var value2);
				dictionary[stat4.Key] = value2 + ((stat4.Value.Bonus != -999) ? stat4.Value.Bonus : 0);
			}
		}
		return dictionary;
	}

	public Dictionary<string, int> GetFinalStats(EmbarkBuilder Builder)
	{
		return GetFinalStats(Builder.GetModule<QudGenotypeModule>()?.data?.Entry, Builder.GetModule<QudSubtypeModule>()?.data?.Entry);
	}

	public Dictionary<string, int> GetFinalStats(EmbarkInfo Info)
	{
		return GetFinalStats(Info.getData<QudGenotypeModuleData>()?.Entry, Info.getData<QudSubtypeModuleData>()?.Entry);
	}

	public Dictionary<string, int> GetFinalStats(GenotypeEntry GenotypeEntry, SubtypeEntry SubtypeEntry)
	{
		Dictionary<string, int> typeStats = GetTypeStats(GenotypeEntry, SubtypeEntry);
		Dictionary<string, int> dictionary = data?.PointsPurchased;
		if (dictionary != null)
		{
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				typeStats.TryGetValue(item.Key, out var value);
				typeStats[item.Key] = item.Value + value;
			}
		}
		return typeStats;
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		if (id == QudGameBootModule.BOOTEVENT_BEFOREBOOTPLAYEROBJECT && element is GameObject gameObject)
		{
			foreach (KeyValuePair<string, int> finalStat in GetFinalStats(info))
			{
				gameObject.GetStat(finalStat.Key).BaseValue = finalStat.Value;
			}
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public override SummaryBlockData GetSummaryBlock()
	{
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		foreach (KeyValuePair<string, int> finalStat in GetFinalStats(builder))
		{
			stringBuilder.Compound(Statistic.GetStatCapitalizedDisplayName(finalStat.Key), '\n').Append(": ").Append(finalStat.Value);
		}
		return new SummaryBlockData
		{
			Id = GetType().FullName,
			Title = "Attributes",
			Description = stringBuilder.ToString(),
			SortOrder = -100
		};
	}
}
