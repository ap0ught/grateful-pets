using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI.Framework;
using XRL.World;

namespace XRL.CharacterBuilds.Qud;

public class QudChooseStartingLocationModule : QudEmbarkBuilderModule<QudChooseStartingLocationModuleData>
{
	private StartingLocationData currentReadingStartingLocationData;

	public Dictionary<string, StartingLocationData> startingLocations = new Dictionary<string, StartingLocationData>();

	public override Dictionary<string, Action<XmlDataHelper>> XmlNodes
	{
		get
		{
			Dictionary<string, Action<XmlDataHelper>> xmlNodes = base.XmlNodes;
			xmlNodes.Add("locations", (XmlDataHelper xml) =>
			{
				xml.HandleNodes(XmlNodeHandlers);
			});
			return xmlNodes;
		}
	}

	public Dictionary<string, Action<XmlDataHelper>> XmlNodeHandlers => new Dictionary<string, Action<XmlDataHelper>>
	{
		{
			"location",
			(XmlDataHelper xml) =>
			{
				string attribute = xml.GetAttribute("ID");
				if (!startingLocations.TryGetValue(attribute, out currentReadingStartingLocationData))
				{
					currentReadingStartingLocationData = new StartingLocationData
					{
						Id = attribute
					};
					startingLocations.Add(attribute, currentReadingStartingLocationData);
				}
				currentReadingStartingLocationData.Name = xml.GetAttributeString("Name", currentReadingStartingLocationData.Name);
				currentReadingStartingLocationData.Location = xml.GetAttributeString("Location", currentReadingStartingLocationData.Location);
				currentReadingStartingLocationData.Set = xml.GetAttributeString("Set", currentReadingStartingLocationData.Set);
				currentReadingStartingLocationData.ExcludeFromDaily = xml.GetAttributeString("ExcludeFromDaily", currentReadingStartingLocationData.ExcludeFromDaily);
				xml.HandleNodes(XmlNodeHandlers);
				currentReadingStartingLocationData = null;
			}
		},
		{
			"description",
			(XmlDataHelper xml) =>
			{
				currentReadingStartingLocationData.Description = xml.GetTextNode();
			}
		},
		{
			"stringgamestate",
			(XmlDataHelper xml) =>
			{
				currentReadingStartingLocationData.stringGameStates[xml.GetAttribute("Name")] = xml.GetAttribute("Value");
				xml.DoneWithElement();
			}
		},
		{
			"item",
			(XmlDataHelper xml) =>
			{
				currentReadingStartingLocationData.items.Add(new StartingLocationItem
				{
					Blueprint = xml.GetAttribute("Blueprint"),
					Number = ((xml.GetAttribute("Number") == null) ? 1 : int.Parse(xml.GetAttribute("Number")))
				});
				xml.DoneWithElement();
			}
		},
		{
			"skill",
			(XmlDataHelper xml) =>
			{
				currentReadingStartingLocationData.skills.Add(new StartingLocationSkill
				{
					Class = xml.GetAttribute("Class")
				});
				xml.DoneWithElement();
			}
		},
		{
			"reputation",
			(XmlDataHelper xml) =>
			{
				currentReadingStartingLocationData.reputations.Add(new StartingLocationReputation
				{
					Faction = xml.GetAttribute("Faction"),
					Modifier = ((xml.GetAttribute("Modifier") != null) ? int.Parse(xml.GetAttribute("Modifier")) : 0)
				});
				xml.DoneWithElement();
			}
		},
		{
			"grid",
			(XmlDataHelper xml) =>
			{
				currentReadingStartingLocationData.grid[xml.GetAttribute("Position")] = new StartingLocationGridElement
				{
					Tile = xml.GetAttribute("Tile"),
					Background = xml.GetAttribute("Background"),
					Detail = xml.GetAttribute("Detail"),
					Foreground = xml.GetAttribute("Foreground")
				};
				xml.DoneWithElement();
			}
		}
	};

	public StartingLocationData startingLocation => startingLocations?.Values.Where((StartingLocationData s) => s.Id == data?.StartingLocation).FirstOrDefault();

	public override bool shouldBeEnabled()
	{
		return builder?.GetModule<QudSubtypeModule>()?.data?.Subtype != null;
	}

	public override bool shouldBeEditable()
	{
		return builder?.GetModule<QudChartypeModule>()?.data?.type != "Daily";
	}

	public override void InitFromSeed(string Seed)
	{
	}

	public override void bootGame(XRLGame Game, EmbarkInfo Info)
	{
	}

	public override object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element)
	{
		if (id == QudGameBootModule.BOOTEVENT_BOOTPLAYEROBJECT)
		{
			GameObject gameObject = element as GameObject;
			foreach (StartingLocationSkill skill in startingLocation.skills)
			{
				gameObject.AddSkill(skill.Class);
			}
			foreach (StartingLocationItem item in startingLocation.items)
			{
				AbstractQudEmbarkBuilderModule.AddItem(item.Blueprint, item.Number, gameObject);
			}
			foreach (StartingLocationReputation reputation in startingLocation.reputations)
			{
				The.Game.PlayerReputation.Modify(reputation.Faction, reputation.Modifier, "StartingLocation", null, null, Silent: true);
			}
		}
		if (id == QudGameBootModule.BOOTEVENT_BEFOREINITIALIZEHISTORY)
		{
			game.SetStringGameState("embark", info.getData<QudSubtypeModuleData>()?.Entry?.StartingLocation ?? info.getData<QudGenotypeModuleData>()?.Entry?.StartingLocation ?? data.StartingLocation);
			foreach (KeyValuePair<string, string> stringGameState in startingLocation.stringGameStates)
			{
				game.SetStringGameState(stringGameState.Key, stringGameState.Value);
			}
		}
		if (id == QudGameBootModule.BOOTEVENT_BOOTSTARTINGLOCATION)
		{
			string text = info.getData<QudSubtypeModuleData>()?.Entry?.StartingLocation ?? info.getData<QudGenotypeModuleData>()?.Entry?.StartingLocation;
			if (!text.IsNullOrEmpty())
			{
				return new GlobalLocation(text);
			}
			text = startingLocation.Location;
			if (!text.IsNullOrEmpty())
			{
				if (text.StartsWith("GlobalLocation:"))
				{
					return new GlobalLocation(text.Split(':')[1]);
				}
				if (text.StartsWith("StringGameState:"))
				{
					return new GlobalLocation(game.GetStringGameState(text.Split(':')[1], "JoppaWorld.11.22.1.1.10@37,22"));
				}
			}
			MetricsManager.LogError("unknown starting location specification:" + text);
			throw new ArgumentException("Starting location was not properly defined for QudChooseStartingLocationModule.");
		}
		return base.handleBootEvent(id, game, info, element);
	}

	public string getSelected()
	{
		return data?.StartingLocation;
	}
}
