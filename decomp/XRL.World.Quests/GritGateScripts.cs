using System;
using XRL.Wish;
using XRL.World.Parts;
using XRL.World.ZoneParts;

namespace XRL.World.Quests;

[HasWishCommand]
public static class GritGateScripts
{
	public static readonly string FACTION = "Barathrumites";

	public static void PromoteToApprentice()
	{
		The.Game.PlayerReputation.PromoteIfBelow(FACTION, "apprentice");
		OpenRank1Doors();
		IdentifyByTag("GritGateApprenticeIdentify");
	}

	public static void PromoteToJourneyfriend()
	{
		The.Game.PlayerReputation.PromoteIfBelow(FACTION, "journeyfriend");
		OpenRank2Doors();
		IdentifyByTag("GritGateJourneyfriendIdentify");
	}

	public static void PromoteToDisciple()
	{
		The.Game.PlayerReputation.PromoteIfBelow(FACTION, "disciple");
		OpenRank2Doors();
		IdentifyByTag("GritGateDiscipleIdentify");
	}

	public static void PromoteToMeyvn()
	{
		The.Game.PlayerReputation.PromoteIfBelow(FACTION, "meyvn");
		OpenRank2Doors();
		IdentifyByTag("GritGateMeyvnIdentify");
	}

	public static void IdentifyByTag(string Tag)
	{
		The.Player?.CurrentZone?.ForeachObject((GameObject GO) =>
		{
			if (GO.HasTagOrProperty(Tag))
			{
				GO.MakeUnderstood();
				GO.ForeachInventoryAndEquipment((GameObject OGO) =>
				{
					OGO.MakeUnderstood();
				});
			}
		});
	}

	public static void CheckGritGateDoors()
	{
		int num = The.Game.GetIntGameState("GritGateDoorAccess", -1);
		if (num == -1)
		{
			num = Math.Min(The.Game.PlayerReputation.GetFactionStanding(FACTION), 2);
		}
		if (num >= 0)
		{
			OpenGritGateDoors(num);
		}
	}

	public static void OpenGritGateDoors(int Rank)
	{
		Zone zone = The.ZoneManager.GetZone("JoppaWorld.22.14.1.0.13");
		for (int i = 0; i <= Rank; i++)
		{
			string name = "GritGateDoorRank" + i;
			foreach (GameObject item in zone.YieldObjects())
			{
				if (!item.HasTagOrProperty(name))
				{
					continue;
				}
				if (item.TryGetPart<Door>(out var Part))
				{
					Part.Locked = false;
					if (!Part.Open)
					{
						Part.PerformOpen();
					}
				}
				if (item.TryGetPart<ForceProjector>(out var Part2))
				{
					Part2.AddAllowPassage("Player");
				}
			}
		}
		int intGameState = The.Game.GetIntGameState("GritGateDoorAccess", -1);
		if (Rank > intGameState)
		{
			The.Game.SetIntGameState("GritGateDoorAccess", Rank);
		}
	}

	public static void OpenRank0Doors()
	{
		OpenGritGateDoors(0);
	}

	[WishCommand("GritGateDoors", null)]
	public static void OpenRank1Doors()
	{
		OpenGritGateDoors(1);
	}

	[WishCommand("GritGateDoors+", null)]
	public static void OpenRank2Doors()
	{
		OpenGritGateDoors(2);
	}

	public static void BeginInvasion()
	{
		if (!The.Game.HasGameState("CallToArmsStarted"))
		{
			The.Player.CurrentZone.RequirePart<ScriptCallToArms>();
		}
	}
}
