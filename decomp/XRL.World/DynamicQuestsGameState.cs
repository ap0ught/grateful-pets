using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.UI;
using XRL.Wish;
using XRL.World.WorldBuilders;
using XRL.World.ZoneBuilders;

namespace XRL.World;

[Serializable]
[GameStateSingleton]
[HasWishCommand]
public class DynamicQuestsGameState : IGameStateSingleton, IComposite
{
	public long NextID;

	public Dictionary<string, Quest> Quests = new Dictionary<string, Quest>();

	public static DynamicQuestsGameState Instance => XRLCore.Core.Game.RequireGameState("DynamicQuestsGameState", () => new DynamicQuestsGameState());

	public bool WantFieldReflection => false;

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(NextID);
		Writer.WriteOptimized(Quests.Count);
		foreach (KeyValuePair<string, Quest> quest in Quests)
		{
			Writer.WriteOptimized(quest.Key);
			quest.Value.Save(Writer);
		}
	}

	public void Read(SerializationReader Reader)
	{
		NextID = Reader.ReadOptimizedInt64();
		int num = Reader.ReadOptimizedInt32();
		for (int i = 0; i < num; i++)
		{
			Quests.Add(Reader.ReadOptimizedString(), Quest.Load(Reader));
		}
	}

	public void LegacyRead(SerializationReader reader)
	{
		NextID = reader.ReadInt64();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Quests.Add(reader.ReadString(), Quest.Load(reader));
		}
	}

	public static void Add(Quest Quest)
	{
		Instance.Quests.Add(Quest.ID, Quest);
	}

	[WishCommand("godynamicquest", null)]
	public static void Wish(string Param)
	{
		Popup.Suppress = true;
		try
		{
			Param.Split(':', out var First, out var Second);
			if (Second.IsNullOrEmpty() || Second.Contains("giver", StringComparison.OrdinalIgnoreCase))
			{
				FindVillageQuest(First);
			}
			else if (Second.Contains("target", StringComparison.OrdinalIgnoreCase))
			{
				FindQuestTarget(First);
			}
		}
		finally
		{
			Popup.Suppress = false;
		}
	}

	private static void FindQuestTarget(string Type)
	{
		foreach (KeyValuePair<string, Quest> quest2 in Instance.Quests)
		{
			quest2.Deconstruct(out var key, out var value);
			Quest quest = value;
			if (!The.Game.Quests.TryGetValue(quest.ID, out var Value) || Value.StepsByID.Any((KeyValuePair<string, QuestStep> x) => x.Value.Finished))
			{
				continue;
			}
			QuestManager manager = quest.Manager;
			if (manager is FindASiteDynamicQuestManager findASiteDynamicQuestManager && Type.Contains("site", StringComparison.OrdinalIgnoreCase))
			{
				Cell randomElement = The.ZoneManager.GetZone(findASiteDynamicQuestManager._zoneID).GetEmptyCells().GetRandomElement();
				The.Player.DirectMoveTo(randomElement, 0, Forced: true);
				return;
			}
			if ((!(manager is FindASpecificItemDynamicQuestManager) || !Type.Contains("item", StringComparison.OrdinalIgnoreCase)) && (!(manager is InteractWithAnObjectDynamicQuestManager) || !Type.Contains("interact", StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}
			string text = (manager as FindASpecificItemDynamicQuestManager)?._itemID ?? (manager as InteractWithAnObjectDynamicQuestManager)?._itemID;
			foreach (KeyValuePair<string, ZoneBuilderCollection> zoneBuilder in The.ZoneManager.ZoneBuilders)
			{
				zoneBuilder.Deconstruct(out key, out var value2);
				string zoneID = key;
				ZoneBuilderCollection zoneBuilderCollection = value2;
				int num = 0;
				for (int count = zoneBuilderCollection.Count; num < count; num++)
				{
					ZoneBuilderBlueprint zoneBuilderBlueprint = zoneBuilderCollection[num];
					if (!zoneBuilderBlueprint.Class.EndsWith("FabricateQuestItem"))
					{
						continue;
					}
					string text2 = (string)zoneBuilderBlueprint.GetParameter("deliveryItemID");
					if (text2 == text)
					{
						GameObject gameObject = The.ZoneManager.GetZone(zoneID).FindObjectByID(text2);
						The.Player.DirectMoveTo(gameObject.GetCurrentCell().GetFirstAdjacentCell((Cell x) => x.IsEmptyFor(The.Player)) ?? gameObject.CurrentCell, 0, Forced: true);
						return;
					}
				}
			}
		}
		Popup.Show("Matching quest target not found.");
	}

	private static void FindVillageQuest(string Type)
	{
		foreach (GeneratedLocationInfo village in ((WorldInfo)The.Game.GetObjectGameState("JoppaWorldInfo")).villages)
		{
			string targetZone = village.targetZone;
			if (The.ZoneManager.IsZoneBuilt(targetZone))
			{
				continue;
			}
			ZoneBuilderCollection builderCollection = The.ZoneManager.GetBuilderCollection(targetZone);
			if (builderCollection == null)
			{
				continue;
			}
			int i = 0;
			for (int count = builderCollection.Count; i < count; i++)
			{
				ZoneBuilderBlueprint zoneBuilderBlueprint = builderCollection[i];
				if (zoneBuilderBlueprint.Class.EndsWith("FabricateQuestGiver") && ((zoneBuilderBlueprint.Class.StartsWith("FindASpecificSite") && Type.Contains("site", StringComparison.OrdinalIgnoreCase)) || (zoneBuilderBlueprint.Class.StartsWith("FindASpecificItem") && Type.Contains("item", StringComparison.OrdinalIgnoreCase)) || (zoneBuilderBlueprint.Class.StartsWith("InteractWithAnObject") && Type.Contains("interact", StringComparison.OrdinalIgnoreCase))))
				{
					GoToQuestGiver(targetZone, Type);
					return;
				}
			}
		}
	}

	private static void GoToQuestGiver(string ZoneID, string Type)
	{
		Zone.ObjectEnumerator enumerator = The.ZoneManager.GetZone(ZoneID).IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			if (current.Brain == null)
			{
				continue;
			}
			string stringProperty = current.GetStringProperty("GivesDynamicQuest");
			if (stringProperty.IsNullOrEmpty() || !Instance.Quests.TryGetValue(stringProperty, out var value))
			{
				continue;
			}
			QuestManager manager = value.Manager;
			if ((manager is FindASiteDynamicQuestManager && Type.Contains("site", StringComparison.OrdinalIgnoreCase)) || (manager is FindASpecificItemDynamicQuestManager && Type.Contains("item", StringComparison.OrdinalIgnoreCase)) || (manager is InteractWithAnObjectDynamicQuestManager && Type.Contains("interact", StringComparison.OrdinalIgnoreCase)))
			{
				Cell targetCell = current.CurrentCell.GetFirstAdjacentCell((Cell x) => x.IsEmptyFor(The.Player)) ?? current.CurrentCell;
				The.Player.DirectMoveTo(targetCell, 0, Forced: true, IgnoreCombat: true);
				break;
			}
		}
	}
}
